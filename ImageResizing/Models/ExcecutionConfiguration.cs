using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizing.Models
{
    public class ExcecutionConfiguration
    {
        public int Width { get; set; }
        public int Height { get; set; }


        public int ThreadCount { get; set; }
        public bool IsAssembly { get; set; }

        public Bitmap InitialBitmap { get; set; }
    }
}
