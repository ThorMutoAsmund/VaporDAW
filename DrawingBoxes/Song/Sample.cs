using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public struct Sample
    {
        public float Left;
        public float Right;

        public void Add(Sample other)
        {
            this.Left += other.Left;
            this.Right += other.Right;
        }
    }
}
