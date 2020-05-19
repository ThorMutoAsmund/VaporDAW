using VaporDAW;
using System.Linq;


public class DefaultPart : Processor
{
    private Channel mainOutput;

    private double start;
    private double length;
    private double end;

    public override void Init()
    {
        var part = this.Song.Parts.FirstOrDefault(p => p.Id == this.ElementId);
        this.start = part.Start;
        this.length = part.Length;
        this.end = this.start + this.length;

        var lastGenerator = part.Generators.Last();
        if (lastGenerator != null)
        {
            var i = 0;
            var processor = this.Env.Processors[lastGenerator.Id];
            this.SetInput($"P{i++}", Tags.MainOutput, processor);
        }

        this.mainOutput = this.AddOutputChannel(Tags.MainOutput);
    }

    public override Mode Process(ProcessParams p)
    {
        if (p.End < this.start || p.Start > this.end || this.Inputs.Count == 0)
        {
            return Mode.Silence;
        }

        var result = Mode.Silence;
        this.mainOutput.Clear(p.NumSamples);

        //Channel previousInput = default;
        //var lastInput = this.InputChannels.Last();

        foreach (var inputChannel in this.Inputs)
        {
            if (inputChannel.Provider.ProcessResult == Mode.ReadWrite)
            {
                this.mainOutput.Add(inputChannel.ProviderOutputChannel);
                result = Mode.ReadWrite;
            }
        }

        return result;

        //foreach (var inputChannel in this.InputChannels)
        //{
        //    if (previousInput != null)
        //    {
        //        inputChannel.SetInput(Tags.MainInput, Tags.MainOutput, previousInput);
        //    }

        //    if (inputChannel == lastInput && inputChannel.Process(p) == Mode.ReadWrite)
        //    {
        //        this.mainOutput.Add(inputChannel.GetOutput(Tags.MainOutput));
        //    }

        //    previousInput = inputChannel;
        //}
    }
}
