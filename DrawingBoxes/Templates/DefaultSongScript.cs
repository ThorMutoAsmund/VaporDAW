using VaporDAW;
using System.Linq;

public class DefaultMixer : Processor
{
    private Channel mainOutput;

    public override void Init(ProcessParams p)
    {
        var i = 0;
        var tracks = this.Song.Tracks.Any(track => track.IsSolo) ?
            this.Song.Tracks.Where(track => track.IsAudible && track.IsSolo) :
            this.Song.Tracks.Where(track => track.IsAudible && !track.IsMuted);
        foreach (var input in tracks.Select(track => this.Env.Processors[track.Id]))
        {
            this.SetInput($"M{i++}", Tags.MainOutput, input);
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
                this.mainOutput.Add(input.ProviderOutputChannel);
                result = Mode.ReadWrite;
            }
        }

        return result;
    }
}
