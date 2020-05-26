using System;
using System.Linq;
using VaporDAW;

// cf: https://github.com/naudio/NAudio

public class DefaultSample : Processor
{
    private Channel mainOutput;
    private ProcessorInput input;

    //private SampleRef sampleRef;
    public override void Init(ProcessParams p)
    {
        var generator = this.Part.Generators.Single(g => g.Id == this.ElementId);
        var sampleId = generator.Settings[Tags.SampleId] as string;
        //this.sampleRef = Env.Song.GetSampleRef(sampleId);

        this.mainOutput = AddOutputChannel(Tags.MainOutput);

        var processor = this.Env.Processors[sampleId];
        this.input = this.SetInput($"S0", Tags.MainOutput, processor);

        // Tbd UP/DOWN sample
    }

    public override Mode Process(ProcessParams p)
    {
        var length = Math.Min(this.input.ProviderOutputChannel.SampleLength, this.Part.SampleLength);
        this.mainOutput.SetRange(this.input.ProviderOutputChannel, 0, 0, length);
        
        return Mode.ReadWrite;
    }
}
