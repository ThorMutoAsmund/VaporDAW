using VaporDAW;
using System.Linq;

public class DefaultMixer : Processor
{
    private Channel mainOutput;

    public override void Init()
    {
        // TBD sort by dependency
        //env.TrackScriptClasses.Select()
        //this.inputPorts = this.Tracks.Select(track => track.Processor).ToArray();

        //this.TrackProcessors = trackProcessors.ToArray();
        var i = 0;
        foreach (var input in this.Song.Tracks.Select(track => this.Env.Processors[track.Id]))
        {
            this.SetInput($"M{i++}", Tags.MainOutput, input);
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
