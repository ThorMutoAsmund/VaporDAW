using VaporDAW;
using System.Threading.Tasks;

public class DefaultSample : Processor
{
    private Processor[] inputs;
    private Channel mainOutput;

    public override void Init(ProcessEnv env, Song song)
    {
        //var track = song.Tracks.FirstOrDefault(t => t.Id == this.ElementId);
        //this.inputs = track.Parts.Select(part => env.Processors[part.Id]).ToArray();
        //this.mainOutput = this.AddOutputChannel(Tags.MainOutput);
    }

    public override Mode Process(ProcessParams p)
    {
        this.mainOutput.Clear(p.SampleLength);

        foreach (var input in this.inputs)
        {
            input.Process(p);
            this.mainOutput.Add(input.GetOutput(Tags.MainOutput));
        }

        return Mode.ReadWrite;
    }
}
