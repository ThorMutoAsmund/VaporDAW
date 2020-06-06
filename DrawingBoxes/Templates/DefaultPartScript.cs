using VaporDAW;
using System.Linq;
using System;

public class DefaultPart : Processor
{
    private Channel mainOutput;

    private int sampleStart;
    private int sampleLength;

    public override void Init(ProcessParams p)
    {
        var part = this.Song.Parts.FirstOrDefault(pa => pa.Id == this.Part.Id);
        this.sampleStart = part.SampleStart;
        this.sampleLength = part.SampleLength;

        var lastGenerator = part.Generators.LastOrDefault();
        if (lastGenerator != null)
        {
            var i = 0;
            var processor = this.ProcessEnv.Processors[lastGenerator.Id];
            this.SetInput($"P{i++}", Tags.MainOutput, processor);
        }

        this.mainOutput = this.AddOutputChannel(Tags.MainOutput);
    }

    public override Mode Process(ProcessParams p)
    {
        if (this.Inputs.Count == 0)
        {
            return Mode.Silence;
        }

        var result = Mode.Silence;
        this.mainOutput.Clear(this.sampleLength);

        foreach (var inputChannel in this.Inputs)
        {
            if (inputChannel.Provider.ProcessResult == Mode.ReadWrite)
            {
                this.mainOutput.Add(inputChannel.ProviderOutputChannel);

                result = Mode.ReadWrite;
            }
        }

        return result;
    }
}
