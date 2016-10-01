using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimatedSigns
{
    public class Frame
    {
        public Bitmap Bitmap { get; set; }

        public Frame(Bitmap b)
        {
            this.Bitmap = b;
            b.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }
    }
}
