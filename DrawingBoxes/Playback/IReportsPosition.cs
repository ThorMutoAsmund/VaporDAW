using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public interface IPositionedSampleProvider : ISampleProvider
    {
        event Action<long> PositionUpdated;
    }
}
