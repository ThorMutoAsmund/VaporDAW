using VaporDAW;
using System.Linq;

public class DefaultMixer : ProcessorV1
{
    private Channel mainOutput;

    public override void Init(ProcessParamsV1 p)
    {
        var i = 0;
        var tracks = this.Song.Tracks.Any(track => track.IsSolo) ?
            this.Song.Tracks.Where(track => track.IsAudible && track.IsSolo && track.TrackGenerators.Count > 0) :
            this.Song.Tracks.Where(track => track.IsAudible && !track.IsMuted && track.TrackGenerators.Count > 0);
        foreach (var input in tracks.Select(track => this.ProcessEnv.Processors[track.TrackGenerators.Last().Id]))
        {
            this.SetInput($"M{i++}", Tags.MainOutput, input);
        }
                
        this.mainOutput = this.AddOutputChannel(Tags.MainOutput);
    }

    public override Mode Process(ProcessParamsV1 p)
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
