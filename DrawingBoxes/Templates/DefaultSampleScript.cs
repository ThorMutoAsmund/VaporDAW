﻿using System.Linq;
using VaporDAW;

// cf: https://github.com/naudio/NAudio

public class DefaultSample : Processor
{
    private Channel mainOutput;
    private SampleRef sampleRef;
    private Channel data;
    public override void Init()
    {
        var generator = this.Part.Generators.Single(g => g.Id == this.ElementId);
        var sampleId = generator.Settings[Tags.SampleId] as string;
        this.sampleRef = Env.Song.GetSampleRef(sampleId);

        this.mainOutput = AddOutputChannel(Tags.MainOutput);
        this.data = CreateChannel();

        // Read data
        WavFileUtils.ReadWavFile(this.sampleRef.FileName, this.data);
    }

    public override Mode Process(ProcessParams p)
    {
        this.mainOutput.Clear(p.NumSamples);

        // TBD
        

        return Mode.Silence;
    }
}
