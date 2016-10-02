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
            Files = droppedFilePaths;
        }

        /// <summary>
        /// Event handler that creates animated signs for the current files <see cref="Files"/>.
        /// The results are parsed and turned into spawnitem commands, which are then copied to the user's clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
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

            JObject[,] signs;
            try
            {
                animatedFrame.Worker.RunWorkerCompleted += SignWorker_RunWorkerCompleted;
                animatedFrame.Worker.ProgressChanged += SignWorker_ProgressChanged;
                animatedFrame.CreateSigns(FPS, StartIndex);
            }
            catch (ArgumentException aexc)
            {
                MessageBox.Show(aexc.Message);
                return;
            }
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
        private void SignWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
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
            MessageBox.Show("Results copied to Clipboard!");
        }
    }
}
