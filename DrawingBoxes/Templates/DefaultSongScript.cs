using VaporDAW;
using System.Linq;

public class DefaultMixer : Processor
{
    private Processor[] inputs;
    private Channel mainOutput;
    public override void Init(ProcessEnv env, Song song)
    {
        // TBD sort by dependency
        //env.TrackScriptClasses.Select()
        //this.inputPorts = this.Tracks.Select(track => track.Processor).ToArray();

        //this.TrackProcessors = trackProcessors.ToArray();
        this.inputs = song.Tracks.Select(track => env.Processors[track.Id]).ToArray();
        this.mainOutput = this.AddOutputChannel(Tags.MainOutput);

    }

    public override Mode Process(ProcessParams p)
    {
        this.mainOutput.Clear(p.SampleLength);

        foreach (var input in this.inputs)
        {
            if (input.Process(p) == Mode.ReadWrite)
            {
                this.mainOutput.Add(input.GetOutput(Tags.MainOutput));
            }
        }

        return Mode.ReadWrite;
    }
}
