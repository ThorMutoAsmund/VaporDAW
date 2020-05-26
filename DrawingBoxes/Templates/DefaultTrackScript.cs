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
            return part.TrackId == this.ElementId &&
                    !(p.End < part.Start || p.Start > part.End);
        });

        foreach (var inputAndPart in parts.Select(part => (input: this.Env.Processors[part.Id], part)))
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
                if (part.Start >= p.Start)
                {
                    var srcOffset = 0;
                    var destOffset = (int)((part.Start - p.Start) * Env.Song.SampleFrequency);
                    var length = part.End < p.End ? part.SampleLength :
                        p.SampleLength - srcOffset;

                    this.mainOutput.AddRange(input.ProviderOutputChannel, srcOffset, destOffset, length);
                }
                else
                {
                    var srcOffset = (int)((p.Start - part.Start) * Env.Song.SampleFrequency);
                    var destOffset = 0;
                    var length = part.End < p.End ? part.SampleLength - srcOffset :
                        p.SampleLength;

                    this.mainOutput.AddRange(input.ProviderOutputChannel, srcOffset, destOffset, length);
                }
                result = Mode.ReadWrite;
            }
        }

        return result;
    }
}
