using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public abstract class Processor
    {
    }

    public abstract class ProcessorV1 : Processor
    {
        public string GeneratorId { get; private set; }
        
        protected ProcessEnv ProcessEnv { get; private set; }
        protected Song Song { get; private set; }
        protected Part Part { get; private set; }
        protected Track Track { get; private set; }
        public Mode ProcessResult { get; set; } = Mode.Silence;

        public static readonly ProcessorV1 Empty = new EmptyProcessor();
        public Dictionary<string, Channel>.ValueCollection OutputChannels => this.outputChannels.Values;
        public Dictionary<string, ProcessorInput>.ValueCollection Inputs => this.inputs.Values;
        
        public virtual ProcessorConfigV1 Config() { return null; }
        public virtual void Init(ProcessParamsV1 p) { }
        public virtual Mode Process(ProcessParamsV1 p) { return Mode.Silence; }


        private Dictionary<string, Channel> outputChannels = new Dictionary<string, Channel>();

        private Dictionary<string, ProcessorInput> inputs = new Dictionary<string, ProcessorInput>();


        public Channel GetOutputChannel(string tag)
        {
            return this.outputChannels.ContainsKey(tag) ? this.outputChannels[tag] : this.ProcessEnv.EmptyChannel;
        }

        public ProcessorInput SetInput(string inputTag, string outputTag, ProcessorV1 processor, object originator = null)
        {
            var processorInput = new ProcessorInput(processor, outputTag, originator);
            this.inputs[inputTag] = processorInput;
            
            return processorInput;
        }

        protected Channel AddOutputChannel(string tag)
        {
            var result = new Channel(this, tag);
            this.outputChannels[tag] = result;

            return result;
        }
        protected Channel CreateChannel()
        {
            return new Channel(this);
        }

        public void Setup(ProcessEnv env, Song song, string elementId, Part part, Track track)
        {
            this.ProcessEnv = env;
            this.Song = song;
            this.Part = part;
            this.Track = track;
            this.GeneratorId = elementId;
        }

        protected ProcessorConfigV1 Config(Action<ProcessorConfigV1> action)
        {
            var result = new ProcessorConfigV1();
            action?.Invoke(result);
            return result;
        }
        protected List<ConfigParameterV1> Parameters(params ConfigParameterV1[] parameters) => parameters.ToList();

        protected ConfigParameterV1 StringParameter(string name, string label = "") => new ConfigParameterV1(ConfigParameterType.String, name)
        {
            Label = label
        };
        protected ConfigParameterV1 BooleanParameter(string name, string label = "") => new ConfigParameterV1(ConfigParameterType.Boolean, name)
        {
            Label = label
        };
        protected ConfigParameterV1 IntegerParameter(string name, string label = "", int? minValue = null, int? maxValue = null) => new ConfigParameterV1(ConfigParameterType.Integer, name)
        {
            Label = label,
            MinValue = minValue,
            MaxValue = maxValue
        };
        protected ConfigParameterV1 NumberParameter(string name, string label = "", double? minValue = null, double? maxValue = null) => new ConfigParameterV1(ConfigParameterType.Number, name)
        {
            Label = label,
            Scale = NumberScale.Linear,
            MinValue = minValue,
            MaxValue = maxValue
        };
        protected ConfigParameterV1 LogParameter(string name, string label = "") => new ConfigParameterV1(ConfigParameterType.Number, name)
        {
            Label = label,
            Scale = NumberScale.Logarithmic
        };
        protected ConfigParameterV1 TextParameter(string name, string label = "") => new ConfigParameterV1(ConfigParameterType.Text, name)
        {
            Label = label,
        };
        protected ConfigParameterV1 Tab(string label) => new ConfigParameterV1(ConfigParameterType.Tab)
        {
            Label = label
        };
        protected ConfigParameterV1 Section(string label) => new ConfigParameterV1(ConfigParameterType.Section)
        {
            Label = label
        };
        protected ConfigParameterV1 Grid(int columns) => new ConfigParameterV1(ConfigParameterType.Grid)
        {
            Columns = columns
        };
        protected ConfigParameterV1 Grid(double[] columnWidths, double[] rowHeights = null) => new ConfigParameterV1(ConfigParameterType.Grid)
        {
            ColumnWidths = columnWidths,
            RowHeights = rowHeights
        };
    }
}
