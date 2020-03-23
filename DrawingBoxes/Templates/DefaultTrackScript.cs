using VaporDAW;
using System.Linq;
using System.Threading.Tasks;

public class DefaultTrack : Processor
{
    private Processor[] inputs;
    private Channel mainOutput;

    public override void Init(ProcessEnv env, Song song)
    {
        var track = song.Tracks.FirstOrDefault(t => t.Id == this.ElementId);
        this.inputs = song.Parts.Where(part => part.TrackId == track.Id).Select(part => env.Processors[part.Id]).ToArray();
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
