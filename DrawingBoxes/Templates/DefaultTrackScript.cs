using VaporDAW;
using System.Linq;


public class DefaultTrack : Processor
{
    private Channel mainOutput;

    public override void Init(ProcessParams p)
    {
        var i = 0;
        var parts = this.Song.Parts.Where(part =>
        {
            return part.TrackId == this.Track.Id &&
                    !(p.SampleEnd < part.SampleStart || p.SampleStart > part.SampleEnd) &&
                    part.PartGenerators.Count > 0;
        });

        foreach (var inputAndPart in parts.Select(part => (input: this.ProcessEnv.Processors[part.PartGenerators.Last().Id], part)))
        {
            this.SetInput($"T{i++}", Tags.MainOutput, inputAndPart.input, inputAndPart.part);
        }

        this.mainOutput = this.AddOutputChannel(Tags.MainOutput);
    }

    public override Mode Process(ProcessParams p)
    {
        var result = Mode.Silence;
        this.mainOutput.Clear(p.SampleLength);

        foreach (var input in this.Inputs)
        {
            if (input.Provider.ProcessResult == Mode.ReadWrite)
            {
                var part = input.GetOriginator<Part>();
                if (part.SampleStart >= p.SampleStart)
                {
                    var srcOffset = 0;
                    var destOffset = part.SampleStart - p.SampleStart;
                    var length = part.SampleEnd < p.SampleEnd ? part.SampleLength :
                        p.SampleLength - destOffset;

                    this.mainOutput.AddRange(input.ProviderOutputChannel, srcOffset, destOffset, length);
                }
                else
                {
                    var srcOffset = p.SampleStart - part.SampleStart;
                    var destOffset = 0;
                    var length = part.SampleEnd < p.SampleEnd ? part.SampleLength - srcOffset :
                        p.SampleLength;

                    this.mainOutput.AddRange(input.ProviderOutputChannel, srcOffset, destOffset, length);
                }
                result = Mode.ReadWrite;
            }
        }

        return result;
    }
}
