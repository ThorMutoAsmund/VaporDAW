using VaporDAW;
using System.Linq;
using System;

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

        foreach (var inputChannel in this.Inputs)
        {
            if (inputChannel.Provider.ProcessResult == Mode.ReadWrite)
            {
                var srcOffset = 0;
                var destOffset = (int)(p.SampleRate * Math.Max(this.start - p.Start, 0));
                var destLen = p.NumSamples - destOffset;
                var srcLen = (int)(p.SampleRate * this.length);

                this.mainOutput.AddRange(inputChannel.ProviderOutputChannel, 0, destOffset, Math.Min(srcLen, destLen));

                //for (int s = sStart; s < sEnd; ++s)
                //{
                //}
                //this.mainOutput.Add(inputChannel.ProviderOutputChannel);
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
