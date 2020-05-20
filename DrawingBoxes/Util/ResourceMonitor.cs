using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VaporDAW
{
    public class ResourceMonitor
    {
        private TextBlock cpuUsageTextBlock;
        private TextBlock ramUsageTextBlock;
        private Lazy<PerformanceCounter> CpuCounter = new Lazy<PerformanceCounter>(() => new PerformanceCounter("Processor", "% Processor Time", "_Total"));
        private Lazy<PerformanceCounter> RamCounter = new Lazy<PerformanceCounter>(() => new PerformanceCounter("Memory", "Available MBytes"));

        private ResourceMonitor()
        {
        }
                
        public static ResourceMonitor Create(TextBlock cpuUsageTextBlock, TextBlock ramUsageTextBlock)
        {
            var result = new ResourceMonitor()
            {
                cpuUsageTextBlock = cpuUsageTextBlock,
                ramUsageTextBlock = ramUsageTextBlock
            };

            result.Start();

            return result;
        }

        private void Start()
        {
            var timer = new DispatcherTimer();
            timer.Tick += (sender, e) =>
            {
                var cpuUsage = this.CpuCounter.Value.NextValue();
                var ramUsage = this.RamCounter.Value.NextValue();

                this.cpuUsageTextBlock.Text = $"CPU {cpuUsage:0}%";
                this.ramUsageTextBlock.Text = $"Memory {ramUsage:##,#} MB";
            };
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Start();
        }
    }
}
