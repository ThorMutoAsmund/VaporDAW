using VaporDAW;
using System.Linq;


public class DefaultTrack : Processor
{
    private Channel mainOutput;

    public override void Init()
    {
        var i = 0;
        foreach (var input in this.Song.Parts.Where(part => part.TrackId == this.ElementId).Select(part => this.Env.Processors[part.Id]))
        {
            this.SetInput($"T{i++}", Tags.MainOutput, input);
        }

        this.mainOutput = this.AddOutputChannel(Tags.MainOutput);
    }

    public override Mode Process(ProcessParams p)
    {
        var result = Mode.Silence;
        this.mainOutput.Clear(p.NumSamples);

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
