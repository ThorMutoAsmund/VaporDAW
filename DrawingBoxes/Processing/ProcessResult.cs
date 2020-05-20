using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class ProcessResult
    {
        //public string FileName { get; private set; }

        //public GenerateResult()
        //{
        //    this.FileName = Path.GetRandomFileName();
        //}
        public Channel Channel { get; private set; }

        public long ElapsedMilliseconds { get; private set; }
        public ProcessResult(Channel channel, long elapsedMilliseconds)
        {
            this.Channel = channel;
            this.ElapsedMilliseconds = elapsedMilliseconds;
        }
    }
}
