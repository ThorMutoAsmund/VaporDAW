using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public static class MP3FileConverter
    {
        public static void Convert(string sourceFilePath, string destinationFilePath)
        {
            using (Mp3FileReader reader = new Mp3FileReader(sourceFilePath))
            {
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    WaveFileWriter.CreateWaveFile(destinationFilePath, pcmStream);
                }
            }
        }
    }
}
