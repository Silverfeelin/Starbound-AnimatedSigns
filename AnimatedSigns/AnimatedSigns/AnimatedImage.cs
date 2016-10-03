using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Silverfeelin.StarboundDrawables;

namespace AnimatedSigns
{
    public class AnimatedImage
    {
        /// <summary>
        /// Collection
        /// </summary>
        public List<Frame> Frames { get; set; }

        public BackgroundWorker Worker { get; set; }

        public string ExportPath { get; set; } = null;

        private int fps;
        private int startIndex;
        private string light;
        private bool wired;

        /// <summary>
        /// Initialized a new empty AnimatedImage.
        /// </summary>
        public AnimatedImage()
        {
            Frames = new List<Frame>();

            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.DoWork += SignWorker_DoWork;
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
        private JObject[,] CreateEmptySigns(int width, int height, int fps = 12, int startIndex = 0, string light = null, bool wired = false)
        {
            JObject[,] signs = new JObject[width, height];
            double cooldown = 1d / fps;

            JObject sign = JObject.Parse("{   \"animationParts\": {     \"background\": \"none?multiply=00000000\"   },   \"scriptStorage\": {},   \"signBacking\": \"none\",   \"signData\": [], \"scriptDelta\": 1,  \"shortdescription\": \"Animated Sign\" }");
            sign["drawCooldown"] = cooldown;
            if (light != null)
                sign["signLight"] = light;

            if (wired)
                sign["isWired"] = true;

            for (int x = 0; x < width; x++) // Every image in the width (32px)
            {
                for (int y = 0; y < height; y++) // Every image in the height (8px)
                {
                    signs[x, y] = (JObject)sign.DeepClone();
                    signs[x, y]["shortdescription"] = string.Format("[{0},{1}]", (x + startIndex), (y + startIndex));
                }
            }

            return signs;
        }

        /// <summary>
        /// Starts creating sign objects containing the parameters for customsigns in a two dimensional array.
        /// The result will be sent to subscribers to the RunWorkerCompleted event of <see cref="Worker"/>.
        /// </summary>
        /// <param name="fps">Animation 'framerate'. Interval between each frame is (1/fps).</param>
        /// <param name="startIndex">Start index used for naming items. "Sign [x,y]"</param>
        public void CreateSigns(int fps, int startIndex = 0, string light = null, bool wired = false, string exportPath = null)
        {
            if (Frames.Count == 0)
                throw new ArgumentException("No frames could be found. Did you select valid files?");

            this.fps = fps;
            this.startIndex = startIndex;
            this.light = light;
            this.ExportPath = exportPath;
            this.wired = wired;

            Worker.RunWorkerAsync(exportPath);
        }

        /// <summary>
        /// BackgroundWorker task. Creates customsign objects containing signData for all <see cref="Frames"/>.
        /// Reports progress every 32x8 processed pixels.
        /// Stores the sign objects (JObject[,]) in e.Result.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SignWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap firstFrame = Frames[0].Bitmap;
            int spriteWidth = (int)Math.Ceiling((decimal)firstFrame.Width / 32);
            int spriteHeight = (int)Math.Ceiling((decimal)firstFrame.Height / 8);
            
            JObject[,] signs = CreateEmptySigns(spriteWidth, spriteHeight, fps, startIndex, light, wired);

            Color[,] templateColors = ColorExt.CreateTemplate();

            int frameCount = 0;
            int maxFrames = Frames.Count;
            DrawablesGenerator.PixelLimit = int.MaxValue;
            foreach (Frame f in Frames)
            {
                Worker.ReportProgress(100 * frameCount++ / maxFrames);
                DrawablesOutput outp = (new DrawablesGenerator(f.Bitmap).Generate());

                for (int i = 0; i < outp.Drawables.GetLength(0); i++)
                {
                    for (int j = 0; j < outp.Drawables.GetLength(1); j++)
                    {
                        ((JArray)signs[i, j]["signData"]).Add(outp.Drawables[i,j].Directives.ToString());
                    }
                }
            }

            Worker.ReportProgress(100);
            if (e.Argument is string)
                e.Result = new Tuple<string, JObject[,]>(e.Argument as string, signs);
            else
                e.Result = signs;

        }
    }
}
