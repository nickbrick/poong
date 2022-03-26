using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    public class Size
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public Size(float width, float height)
        {
            Width = width;
            Height = height;
        }
        public Size(float side)
        {
            Width = side;
            Height = side;
        }
    }
}
