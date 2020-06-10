using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Boolean
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
        private double minValue;
        public double MinValue 
        {
            get => this.minValue;
            set
            {
                this.minValue = value;
                this.intMinValue = (int)value;
            }
        }
        private int intMinValue;
        public int IntMinValue
        {
            get => this.intMinValue;
            set
            {
                this.intMinValue = value;
                this.minValue = value;
            }
        }

        private double maxValue;
        public double MaxValue
        {
            get => this.maxValue;
            set
            {
                this.maxValue = value;
                this.intMaxValue = (int)value;
            }
        }
        private int intMaxValue;
        public int IntMaxValue
        {
            get => this.intMaxValue;
            set
            {
                this.intMaxValue = value;
                this.maxValue = value;
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
