using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace AnimatedSigns
{
    public class AnimatedImage
    {
        /// <summary>
        /// Collection
        /// </summary>
        public List<Frame> Frames { get; set; }

        /// <summary>
        /// Initialized a new empty AnimatedImage.
        /// </summary>
        public AnimatedImage()
        {
            Frames = new List<Frame>();
        }

        /// <summary>
        /// Adds the image frame to this AnimatedImage.
        /// Adding frames of GIF images possible thanks to this answer by nobugz:
        /// https://social.microsoft.com/Forums/en-US/fcb7d14d-d15b-4336-971c-94a80e34b85e/editing-animated-gifs-in-c?forum=netfxbcl
        /// <param name="path">Image file path. Invalid files are ignored.</param>
        /// </summary>
        public void AddImage(string path)
        {
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                if (fi.Extension.ToLower() == ".gif")
                {
                    Image img = Image.FromFile(path);
                    MatchSize(img);

                    int frames = img.GetFrameCount(FrameDimension.Time);
                    byte[] times = img.GetPropertyItem(0x5100).Value;
                    int frame = 0;
                    for (;;)
                    {
                        img.SelectActiveFrame(FrameDimension.Time, frame);
                        int dur = BitConverter.ToInt32(times, 4 * frame);
                        Frames.Add(new Frame(new Bitmap(img)));
                        if (++frame >= frames) break;
                    }
                    img.Dispose();
                }
                else
                {
                    Bitmap b = new Bitmap(path);
                    MatchSize(b);
                    Frames.Add(new Frame(b));
                }
            }
        }

        /// <summary>
        /// Calls <see cref="AddImage(string)"/> for all files in the given collection.
        /// </summary>
        /// <param name="paths">Collection of image file paths.</param>
        public void AddImages(string[] paths)
        {
            foreach (var item in paths)
                AddImage(item);
        }

        /// <summary>
        /// Checks if the given image matches existing frames. Should be called prior to adding any image to <see cref="Frames"/>.
        /// </summary>
        /// <exception cref="DimensionException">Thrown when the given image does not match the dimensions of existing images in <see cref="Frames"/>.</exception>
        /// <param name="img">Image to check dimensions for.</param>
        private void MatchSize(Image img)
        {
            if (Frames.Count > 0 && img.Size != Frames[0].Bitmap.Size)
                throw new DimensionException(string.Format("An image does not have the same dimensions as the first frame!\nExpected: {0}\nGot: {1}", Frames[0].Bitmap.Size, img.Size));
        }

        /// <summary>
        /// Creates and returns two dimensional array of instantiated customsign objects.
        /// signData is left empty.
        /// </summary>
        /// <param name="width">Width of the array (How many horizontal signs are needed for the image?).</param>
        /// <param name="height">Height of the array (How many vertical signs are needed for the image?)</param>
        /// <param name="fps">Animation 'framerate'. Interval between each frame is (1/fps).</param>
        /// <param name="startIndex">Start index used for naming items. "Sign [x,y]"</param>
        /// <returns>Two dimensional array of instantiated customsign objects, without any signData entries.</returns>
        private JObject[,] CreateEmptySigns(int width, int height, int fps = 12, int startIndex = 0)
        {
            JObject[,] signs = new JObject[width, height];
            double cooldown = 1d / fps;

            JObject sign = JObject.Parse("{   \"animationParts\": {     \"background\": \"none?multiply=00000000\"   },   \"scriptStorage\": {},   \"signBacking\": \"none\",   \"signData\": [], \"scriptDelta\": 1,  \"shortdescription\": \"Animated Sign\" }");
            sign["drawCooldown"] = cooldown;

            for (int x = 0; x < width; x++) // Every image in the width (32px)
            {
                for (int y = 0; y < height; y++) // Every image in the height (8px)
                {
                    signs[x, y] = (JObject)sign.DeepClone();
                    signs[x, y]["shortdescription"] = string.Format("Sign [{0},{1}]", (x + startIndex), (y + startIndex));
                }
            }

            return signs;
        }

        /// <summary>
        /// Creates and returns objects containing the parameters for customsigns in a two dimensional array.
        /// </summary>
        /// <param name="fps">Animation 'framerate'. Interval between each frame is (1/fps).</param>
        /// <param name="startIndex">Start index used for naming items. "Sign [x,y]"</param>
        /// <returns>Two dimensional array of configured customsign objects.</returns>
        public JObject[,] CreateSigns(int fps, int startIndex = 0)
        {
            if (Frames.Count == 0)
                throw new ArgumentException("No frames could be found. Did you select valid files?");

            Bitmap firstFrame = Frames[0].Bitmap;
            int spriteWidth = (int)Math.Ceiling((decimal)firstFrame.Width / 32);
            int spriteHeight = (int)Math.Ceiling((decimal)firstFrame.Height / 8);

            JObject[,] signs = CreateEmptySigns(spriteWidth, spriteHeight, fps, startIndex);

            Color[,] templateColors = ColorExt.CreateTemplate();
            
            foreach (Frame f in Frames)
            {
                Point imagePixel = new Point(0, 0);

                // Add a drawable for every signplaceholder needed.
                for (int frameWidth = 0; frameWidth < spriteWidth; frameWidth++)
                {
                    for (int frameHeight = 0; frameHeight < spriteHeight; frameHeight++)
                    {
                        imagePixel = new Point(frameWidth * 32, frameHeight * 8);

                        bool containsPixels = false;
                        
                        StringBuilder directives = new StringBuilder("?replace");

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

                                Color imageColor = f.Bitmap.GetPixel(Convert.ToInt32(imagePixel.X), Convert.ToInt32(imagePixel.Y));

                                Color templateColor = templateColors[i, j];

                                if (imageColor.A > 1)
                                    directives.AppendFormat(";{0}={1}", templateColor.ToRGBAHexString(), imageColor.ToRGBAHexString());

                                imagePixel.Y++;
                            }

                            imagePixel.X++;
                            imagePixel.Y = frameHeight * 8;
                        }
                        
                        ((JArray)signs[frameWidth, frameHeight]["signData"]).Add(directives.ToString());
                    }
                }
            }

            return signs;
        }
    }
}
