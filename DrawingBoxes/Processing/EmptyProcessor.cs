using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class EmptyProcessor : ProcessorV1
    {
        public override Mode Process(ProcessParamsV1 p)
        {
            return Mode.Silence;
        }
    }
}
