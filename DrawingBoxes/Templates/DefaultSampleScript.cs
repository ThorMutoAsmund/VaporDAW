using System;
using System.Linq;
using VaporDAW;

// cf: https://github.com/naudio/NAudio

public class DefaultSample : ProcessorV1
{
    private Channel mainOutput;
    private ProcessorInput input;

    public override ProcessorConfigV1 Config()
    {
        return Config(config =>
        {
            config.Parameters = Parameters(
                Section("Various"),
                StringParameter("navn", label: "Navn"),
                StringParameter("efternavn", label: "Efternavn"),
                Grid(4),
                Section("Integers"),
                IntegerParameter("min", label: "Minimum"),
                IntegerParameter("max", label: "Maximum"),
                Section("Numbers"),
                NumberParameter("min2", label: "Minimum"),
                NumberParameter("max2", label: "Maximum"),
                Section("Bools"),
                Grid(8),
                BooleanParameter("P1", label: "P1"),
                BooleanParameter("P2", label: "P2"),
                BooleanParameter("P3", label: "P3"),
                BooleanParameter("P4", label: "P4")
            );
        });
    }

    public override void Init(ProcessParamsV1 p)
    {
        var generator = this.Part.Generators.Single(g => g.Id == this.GeneratorId);
        var sampleId = generator.Settings[Tags.SampleId] as string;

        this.mainOutput = AddOutputChannel(Tags.MainOutput);

        var processor = this.ProcessEnv.Processors[sampleId];
        this.input = this.SetInput($"S0", Tags.MainOutput, processor);

        // Tbd UP/DOWN sample
    }

    public override Mode Process(ProcessParamsV1 p)
    {
        var length = Math.Min(this.input.ProviderOutputChannel.SampleLength, this.Part.SampleLength);
        this.mainOutput.SetRange(this.input.ProviderOutputChannel, 0, 0, length);
        
        return Mode.ReadWrite;
    }
}
