using System.Linq;
using VaporDAW;

// cf: https://github.com/naudio/NAudio

public class SampleDataProcessor : ProcessorV1
{
    private Channel mainOutput;

    public override void Init(ProcessParamsV1 p)
    { 
        var sampleRef = ProcessEnv.Song.GetSampleRef(this.GeneratorId);
        this.mainOutput = AddOutputChannel(Tags.MainOutput);

        // Read data
        WavFileUtils.ReadWavFile(sampleRef.FileName, this.mainOutput);
    }

    public override Mode Process(ProcessParamsV1 p)
    {
        return Mode.ReadWrite;
    }

    public static int GetSampleLength(string sampleId)
    {
        var sampleRef = Env.Song.GetSampleRef(sampleId);
        return WavFileUtils.GetWavFileSampleLength(sampleRef.FileName);
    }
}
