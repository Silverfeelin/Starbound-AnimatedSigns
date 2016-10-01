using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ns;
using System.IO;
using System.Drawing;
using Newtonsoft.Json.Linq;

namespace AnimatedSigns
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image Frames|*.png;*.jpg;*.gif;*.bmp",
                Multiselect = true,
                CheckFileExists = true
            };

            bool? res = ofd.ShowDialog();
            if (!res.HasValue || !res.Value)
            {
                return;
            }

            string[] files = ofd.FileNames;

            NumericComparer ns = new NumericComparer();
            Array.Sort(files, ns);


            AnimatedImage animatedFrame = new AnimatedImage();
            try
            {
                animatedFrame.AddImages(files);
            }
            catch (DimensionException dexc)
            {
                MessageBox.Show(dexc.Message);
                return;
            }

            CreateSigns(animatedFrame);

            bool b = true;
        }
        
        private void CreateSigns(AnimatedImage animatedFrame)
        {
            Bitmap firstFrame = animatedFrame.Frames[0].Bitmap;
            int spriteWidth = (int)Math.Ceiling((decimal)firstFrame.Width / 32);
            int spriteHeight = (int)Math.Ceiling((decimal)firstFrame.Height / 8);

            JObject[,] signs = new JObject[spriteWidth, spriteHeight];

            int drawableX = 0;
            int drawableY = 0;

            System.Drawing.Color[,] templateColors = new System.Drawing.Color[32, 8];

            for (var i = 0; i < 32; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    var x = i;
                    if (i >= 9)
                        x += 6;
                    if (i >= 19)
                        x += 6;
                    if (i >= 29)
                        x += 6;

                    templateColors[i, j] = System.Drawing.Color.FromArgb(1, x + 1, 0, j + 1);
                }
            }

            for (int x = 0; x < spriteWidth; x++) // Every image in the width (32px)
            {
                for (int y = 0; y < spriteHeight; y++) // Every image in the height (8px)
                {
                    signs[x, y] = JObject.Parse("{   \"animationParts\": {     \"background\": \"none?multiply=00000000\"   },   \"scriptStorage\": {},   \"signBacking\": \"none\",   \"signData\": [],   \"shortdescription\": \"Animated Sign\" }");
                    signs[x, y]["shortdescription"] = string.Format("Sign [{0},{1}]", (x + 1), (y + 1));
                }
            }

            foreach (Frame f in animatedFrame.Frames)
            {
                System.Drawing.Point imagePixel = new System.Drawing.Point(0, 0);

                // Add a drawable for every signplaceholder needed.
                for (int frameWidth = 0; frameWidth < spriteWidth; frameWidth++)
                {
                    for (int frameHeight = 0; frameHeight < spriteHeight; frameHeight++)
                    {
                        imagePixel = new System.Drawing.Point(frameWidth * 32, frameHeight * 8);

                        bool containsPixels = false;

                        string texture = "/objects/outpost/customsign/signplaceholder.png";
                        string directives = "?replace";

                        for (int i = 0; i < 32; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                // Pixel falls within template but is outside of the supplied image.
                                if ((imagePixel.X > f.Bitmap.Width - 1 || imagePixel.Y > f.Bitmap.Height - 1))
                                {
                                    imagePixel.Y++;
                                    continue;
                                }

                                System.Drawing.Color imageColor = f.Bitmap.GetPixel(Convert.ToInt32(imagePixel.X), Convert.ToInt32(imagePixel.Y));

                                if (imageColor.ToArgb() == System.Drawing.Color.White.ToArgb())
                                    imageColor = System.Drawing.Color.FromArgb(255, 254, 254, 254);

                                System.Drawing.Color templateColor = templateColors[i,j];

                                directives += string.Format(";{0}={1}", ColorToRGBAHexString(templateColor), ColorToRGBAHexString(imageColor));

                                if (imageColor.A > 1)
                                    containsPixels = true;

                                imagePixel.Y++;
                            }

                            imagePixel.X++;
                            imagePixel.Y = frameHeight * 8;
                        }


                        if (containsPixels)
                            ((JArray)signs[frameWidth, frameHeight]["signData"]).Add(directives);
                    }
                }
            }

            string outp = "";
            for (int i = 0; i < signs.GetLength(1); i++)
            {
                for (int j = 0; j < signs.GetLength(0); j++)
                {
                    outp += "/spawnitem customsign 1 '" + signs[j, i].ToString(Newtonsoft.Json.Formatting.None) + "'\n";
                }
            }

            Clipboard.SetText(outp);
            MessageBox.Show("Output copied to clipboard.");
        }

        private string ColorToRGBAHexString(System.Drawing.Color c)
        {
            string r = c.R.ToString("X2");
            string g = c.G.ToString("X2");
            string b = c.B.ToString("X2");
            string a = c.A.ToString("X2");
            
            return r + g + b + a;
        }
    }
}
