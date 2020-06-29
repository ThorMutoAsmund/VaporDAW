using System.Collections.Generic;

namespace VaporDAW
{
    public enum ConfigParameterType
    {
        Tab,
        Section,
        Grid,
        String,
        Number,
        Integer,
        Boolean,
        Text
    }

    public enum NumberScale
    {
        Linear,
        Logarithmic
    }

    public class ConfigParameterV1
    {
        public ConfigParameterType Type { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public NumberScale Scale { get; set; }
        private double? minValue;
        public double? MinValue 
        {
            get => this.minValue;
            set
            {
                this.minValue = value;
                if (value.HasValue)
                {
                    this.intMinValue =  (int)value.Value;
                }
            }
        }
        private int? intMinValue;
        public int? IntMinValue
        {
            get => this.intMinValue;
            set
            {
                this.intMinValue = value;
                if (value.HasValue)
                {
                    this.minValue = value.Value;
                }
            }
        }

        private double? maxValue;
        public double? MaxValue
        {
            get => this.maxValue;
            set
            {
                this.maxValue = value;
                if (value.HasValue)
                {
                    this.intMaxValue = (int)value.Value;
                }
            }
        }
        private int? intMaxValue;
        public int? IntMaxValue
        {
            get => this.intMaxValue;
            set
            {
                this.intMaxValue = value;
                if (value.HasValue)
                {
                    this.maxValue = value.Value;
                }
            }
        }

        public int Columns { get; set; }
        public double[] ColumnWidths { get; set; }
        public double[] RowHeights { get; set; }

        public ConfigParameterV1(ConfigParameterType type, string name = "")
        {
            this.Type = type;
            this.Name = name;
        }
    }

    public interface IProcessorConfig
    {
        ProcessorConfigV1 ToCurrent();
    }

    public class ProcessorConfigV1 : IProcessorConfig
    {
        public List<ConfigParameterV1> Parameters { get; set; } = new List<ConfigParameterV1>();

        public ProcessorConfigV1 ToCurrent() => this;
    }
}
