using System.Linq;
using VaporDAW;

// cf: https://github.com/naudio/NAudio

public class SampleDataProcessor : Processor
{
    private Channel mainOutput;

    public override void Init(ProcessParams p)
    { 
        var sampleRef = Env.Song.GetSampleRef(this.ElementId);
        this.mainOutput = AddOutputChannel(Tags.MainOutput);

        // Read data
        WavFileUtils.ReadWavFile(sampleRef.FileName, this.mainOutput);
    }

    public override Mode Process(ProcessParams p)
    {
        return Mode.ReadWrite;
    }
}
