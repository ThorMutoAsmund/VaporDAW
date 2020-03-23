using VaporDAW;
using System.Linq;
using System.Threading.Tasks;

public class DefaultPart : Processor
{
    private Processor[] inputs;
    private Channel mainOutput;

    private double start;
    private double length;
    private double end;
    public override void Init(ProcessEnv env, Song song)
    {
        var part = song.Parts.FirstOrDefault(p => p.Id == this.ElementId);
        this.start = part.Start;
        this.length = part.Length;
        this.end = this.start + this.length;

        this.inputs = part.Generators.Select(generator => env.Processors[generator.Id]).ToArray();
        this.mainOutput = this.AddOutputChannel(Tags.MainOutput);
    }

    public override Mode Process(ProcessParams p)
    {
        if (p.End < this.start || p.Start > this.end || this.inputs.Length == 0)
        {
            return Mode.Silence;
        }

        this.mainOutput.Clear(p.SampleLength);

        Processor previousInput = default;
        var lastInput = this.inputs.Last();

        foreach (var input in this.inputs)
        {
            if (previousInput != null)
            {
                input.SetInput(Tags.MainInput, Tags.MainOutput, previousInput);
            }

            if (input == lastInput && input.Process(p) == Mode.ReadWrite)
            {
                this.mainOutput.Add(input.GetOutput(Tags.MainOutput));
            }

            previousInput = input;
        }

        return Mode.ReadWrite;
    }
}
