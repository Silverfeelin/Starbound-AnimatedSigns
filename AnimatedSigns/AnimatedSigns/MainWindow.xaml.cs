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
        /// <summary>
        /// Initializes the main window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Gets or sets the FPS at which the signs cycle through frames.
        /// This value is directly tied to the contents of <see cref="tbxFPS"/>.
        /// </summary>
        public int FPS
        {
            get
            {
                int fps;
                return int.TryParse(tbxFPS.Text, out fps) ? fps : 12;
            }
            set
            {
                tbxFPS.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets the start index, used for naming signs.
        /// Some people prefer indices to start at 0, others 1.
        /// This value is directly tied to the value of <see cref="chkID"/>.
        /// </summary>
        public int StartIndex
        {
            get
            {
                return (chkID.IsChecked.HasValue && chkID.IsChecked.Value) ? 0 : 1;
            }
        }

        /// <summary>
        /// Gets or sets the selection of files.
        /// This value is directly tied to the contents of <see cref="tbxFiles"/>.
        /// </summary>
        public string[] Files
        {
            get
            {
                return tbxFiles.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                tbxFiles.Text = "";
                foreach (var path in value)
                    tbxFiles.Text += path + Environment.NewLine;
            }
        }

        public string Light
        {
            get
            {
                if (string.IsNullOrWhiteSpace(tbxLight.Text))
                    return null;

                return tbxLight.Text.Replace("#", "");
            }
            set
            {
                tbxLight.Text = value;
            }
        }

        /// <summary>
        /// Event handler that allows users to select a variable amount of image files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
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

            tbxFiles.Text = "";

            string[] files = ofd.FileNames;
            NumericComparer ns = new NumericComparer();
            Array.Sort(files, ns);

            Files = files;
        }

        /// <summary>
        /// Event handler that disables whatever stops us from dragging text in the textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Event handler that allows users to drag files in the textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_Drop(object sender, DragEventArgs e)
        {
            string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            Files = Files.Concat(droppedFilePaths).ToArray();
        }

        /// <summary>
        /// Event handler that creates animated signs for the current files <see cref="Files"/>.
        /// The results are parsed and turned into spawnitem commands, which are then copied to the user's clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmGenerating()) return;

            AnimatedImage animatedFrame = new AnimatedImage();
            try
            {
                animatedFrame.AddImages(Files);
            }
            catch (DimensionException dexc)
            {
                MessageBox.Show(dexc.Message);
                return;
            }
            
            try
            {
                animatedFrame.Worker.RunWorkerCompleted += SignWorker_CompletedText;
                animatedFrame.Worker.ProgressChanged += SignWorker_ProgressChanged;
                animatedFrame.CreateSigns(FPS, StartIndex, Light);
            }
            catch (ArgumentException aexc)
            {
                MessageBox.Show(aexc.Message);
                return;
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmGenerating()) return;

            SaveFileDialog sfd = new SaveFileDialog()
            {
                Title = "Exported files location"
            };

            bool? res = sfd.ShowDialog();
            if (!res.HasValue || !res.Value) return;

            AnimatedImage animatedFrame = new AnimatedImage();
            try
            {
                animatedFrame.AddImages(Files);
            }
            catch (DimensionException dexc)
            {
                MessageBox.Show(dexc.Message);
                return;
            }
            
            try
            {
                animatedFrame.Worker.RunWorkerCompleted += SignWorker_CompletedExports;
                animatedFrame.Worker.ProgressChanged += SignWorker_ProgressChanged;
                animatedFrame.CreateSigns(FPS, StartIndex, Light, sfd.FileName);
            }
            catch (ArgumentException aexc)
            {
                MessageBox.Show(aexc.Message);
                return;
            }
        }

        /// <summary>
        /// Prompts the user to confirm if they want to generate the signs.
        /// </summary>
        /// <returns>True if the user says yes, false if there's no valid images or the user says no.</returns>
        private bool ConfirmGenerating()
        {
            if (Files.Length == 0)
            {
                MessageBox.Show("You have not selected any items!");
                return false;
            }

            foreach (var item in Files)
            {
                try
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(item);

                    int width, height;
                    width = (int)Math.Ceiling(img.Width / 32d);
                    height = (int)Math.Ceiling(img.Height / 8d);

                    string message = string.Format("You are about to create the following amount of signs:\nHorizontal: {0}\nVertical: {1}\nTotal: {2}\nDo you want to continue?", width, height, width * height);
                    MessageBoxResult mbr = MessageBox.Show(message, "Warning", MessageBoxButton.YesNo);
                    return mbr == MessageBoxResult.Yes;
                }
                catch
                {
                    continue;
                }
            }

            return false;
            
        }

        /// <summary>
        /// Callback fired when <see cref="AnimatedImage.Worker"/> reports progress. Updates the progress bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SignWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// Callback fired when <see cref="AnimatedImage.Worker"/> is done creating signs.
        /// Creates spawnitem commands for the signs and copies everything to the Clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event args, contains signs as (JObject[,])e.Result.</param>
        private void SignWorker_CompletedText(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            JObject[,] signs = (JObject[,])e.Result;

            StringBuilder outp = new StringBuilder("// Each line contains one /spawnitem command for a sign. Signs are named after their [X,Y] position.\n");
            for (int i = 0; i < signs.GetLength(1); i++)
            {
                for (int j = 0; j < signs.GetLength(0); j++)
                {
                    if (signs[j, i] == null) continue;

                    if (((JArray)signs[j, i]["signData"]).Count > 0)
                        outp.Append("/spawnitem customsign 1 '" + signs[j, i].ToString(Newtonsoft.Json.Formatting.None) + "'\n");
                }
            }

            Clipboard.SetText(outp.ToString());
            MessageBox.Show("Spawnitem commands copied to Clipboard!");
        }

        /// <summary>
        /// Callback fired when <see cref="AnimatedImage.Worker"/> is done creating signs.
        /// Creates export files and saves them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event args, contains signs as (JObject[,])e.Result.</param>
        private void SignWorker_CompletedExports(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Tuple<string, JObject[,]> result = (Tuple<string,JObject[,]>)e.Result;

            JObject[,] signs = result.Item2;

            FileInfo target = new FileInfo(result.Item1);
            DirectoryInfo path = target.Directory;
            if (!path.Exists)
            {
                MessageBox.Show("The target path does not exist. Files could not be saved.");
                return;
            }

            string name = target.Name;
            
            bool useSub = chkSubFolder.IsChecked.HasValue && chkSubFolder.IsChecked.Value;
            if (useSub)
            {
                DirectoryInfo[] subs = path.GetDirectories().Where(d => d.Name == name).ToArray();
                if (subs.Length == 0)
                    path = path.CreateSubdirectory(name);
                else
                    path = subs[0];
            }

            for (int i = 0; i < signs.GetLength(0); i++)
            {
                for (int j = 0; j < signs.GetLength(1); j++)
                {
                    JObject sign = signs[i, j];
                    string signPath = path.FullName + "\\" + name + sign["shortdescription"].Value<string>() + ".json";
                    if (File.Exists(signPath))
                    {
                        MessageBoxResult mbr = MessageBox.Show("File '" + signPath + "' already exists. Do you want to overwrite it?", "Warning", MessageBoxButton.YesNoCancel);
                        if (mbr == MessageBoxResult.No)
                            continue;
                        else if (mbr != MessageBoxResult.Yes)
                            return;
                    }

                    JObject export = new JObject();
                    export["name"] = "customsign";
                    export["count"] = 1;
                    export["parameters"] = sign;

                    File.WriteAllText(signPath, export.ToString(Newtonsoft.Json.Formatting.Indented));
                }
            }
            
            MessageBox.Show("Files saved!");
        }
    }
}
