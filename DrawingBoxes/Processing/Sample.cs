using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public struct Sample
    {
        public double Left;
        public double Right;

        public void Add(Sample other)
        {
            this.Left += other.Left;
            this.Right += other.Right;
        }
    }
}
