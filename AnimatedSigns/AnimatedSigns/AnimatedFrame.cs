using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimatedSigns
{
    /// <summary>
    /// Based on answer by nobugz.
    /// https://social.microsoft.com/Forums/en-US/fcb7d14d-d15b-4336-971c-94a80e34b85e/editing-animated-gifs-in-c?forum=netfxbcl
    /// </summary>
    public class AnimatedImage
    {
        public List<Frame> Frames { get; set; }
        public AnimatedImage()
        {
            Frames = new List<Frame>();
        }

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
        
        private void MatchSize(Image img)
        {
            if (Frames.Count > 0 && img.Size != Frames[0].Bitmap.Size)
                throw new DimensionException(string.Format("An image does not have the same dimensions as the first frame!\nExpected: {0}\nGot: {1}", Frames[0].Bitmap.Size, img.Size));
        }
        public void AddImages(string[] paths)
        {
            foreach (var item in paths)
                AddImage(item);
        }
    }
}
