using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class EmptyProcessor : Processor
    {
        public override Mode Process(ProcessParams p)
        {
            return Mode.Silence;
        }
    }
}
