using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VaporDAW
{
    public partial class EditGeneratorDialog : Window
    {
        public Generator Generator 
        { 
            get => this.generator;
            private set
            {
                this.generator = value;

                this.scriptSelectControl.Script = Env.Song.GetScriptRef(this.Generator.ScriptId);
                this.dataContext = new Dictionary<string, object>(this.Generator.Settings);
                
                UpdateDataContext();
            }
        }

        private Generator generator;
        private Grid currentMasterGrid;
        private Grid currentGrid = null;
        private int nextGridColumn = 0;
        private Dictionary<string, object> dataContext;

        private EditGeneratorDialog()
        {
            InitializeComponent();

            this.okButton.Click += (sender, e) => OK();
            //this.titleTextBox.Focus();
        }

        public static EditGeneratorDialog Create(Window owner, Generator generator)
        {
            var dialog = new EditGeneratorDialog()
            {
                Owner = owner,
                Generator = generator
            };

            return dialog;
        }

        private void OK()
        {
            this.Generator.ScriptId = this.scriptSelectControl.Script.Id;
            this.Generator.Settings = this.dataContext;

            this.DialogResult = true;
        }

        private void UpdateDataContext()
        {
            var scriptRef = Env.Song.GetScriptRef(this.Generator.ScriptId);

            if (scriptRef == null)
            {
                return;
            }

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var className = scriptRef.FileName;
            var subStringLength = className.IndexOf(".");
            if (subStringLength > -1)
            {
                className = className.Substring(0, subStringLength);
            }
            var type = assembly.GetTypes().First(t => t.Name == className);
            if (type == null)
            {
                return;
            }

            var processor = Activator.CreateInstance(type) as ProcessorV1 ?? ProcessorV1.Empty;
            var methodInfo = type.GetMethod(nameof(ProcessorV1.Config));

            var configObject = methodInfo.Invoke(processor, null);

            if (configObject is IProcessorConfig _processorConfig)
            {
                var processorConfig = _processorConfig.ToCurrent();
                ReadProcessorConfig(processorConfig);
            }
        }

        private void SetControlValue(string parameterName, object value)
        {
            if (String.IsNullOrEmpty(parameterName))
            {
                return;
            }

            this.dataContext[parameterName] = value;
        }

        private void ReadProcessorConfig(ProcessorConfigV1 config)
        {
            this.currentMasterGrid = this.defaultMasterGrid;

            foreach (var parameter in config.Parameters)
            {
                switch (parameter.Type)
                {
                    case ConfigParameterType.Tab: AddTab(parameter); break;
                    case ConfigParameterType.Section: AddSection(parameter); break;
                    case ConfigParameterType.Grid: AddGrid(parameter); break;
                    case ConfigParameterType.String:
                    case ConfigParameterType.Boolean:
                    case ConfigParameterType.Integer:
                    case ConfigParameterType.Number:
                    case ConfigParameterType.Text:
                        AddControlParameter(parameter); break;
                }
            }
        }

        
        private void AddTab(ConfigParameterV1 parameter)
        {
            var tabItem = new TabItem()
            {
                Header = parameter.Label
            };
            this.currentMasterGrid = new Grid()
            {
            };
            tabItem.Content = this.currentMasterGrid;
            this.tabControl.Items.Add(tabItem);

            this.currentGrid = null;
        }

        private void AddGridIfNeeded()
        {
            if (this.currentGrid != null)
            {
                return;
            }
            AddGrid();
        }

        private void AddGrid(ConfigParameterV1 parameter = null)
        {
            // Create row in master grid
            var rowDefinition = new RowDefinition()
            {
                Height = new GridLength(1f, GridUnitType.Auto)
            };
            this.currentMasterGrid.RowDefinitions.Add(rowDefinition);

            // Create grid
            this.currentGrid = new Grid()
            {
                Height = Double.NaN,
                Width = Double.NaN,
                Margin = this.currentMasterGrid.RowDefinitions.Count == 1 ? new Thickness(12f, 12f, 12f, 0f) : new Thickness(12f, 0f, 12f, 0f)
            };

            if (parameter != null && parameter.ColumnWidths != null)
            {
                for (int c = 0; c < Math.Min(parameter.Columns, 16); ++c)
                {
                    this.currentGrid.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(parameter.ColumnWidths[c])
                    });
                }
            }
            else if (parameter != null && parameter.Columns > 2)
            {
                this.currentGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(150f)
                });
                for (int c = 1; c < Math.Min(parameter.Columns, 16); ++c)
                {
                    this.currentGrid.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                    });
                }
            }
            else if (parameter != null && parameter.Columns == 1)
            {
                this.currentGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }
            else
            {
                this.currentGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(150f)
                });
                this.currentGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                });
            }

            Grid.SetRow(this.currentGrid, this.currentMasterGrid.RowDefinitions.Count - 1);
            this.currentMasterGrid.Children.Add(this.currentGrid);
            this.currentGrid.Height = Double.NaN;
            this.nextGridColumn = this.currentGrid.ColumnDefinitions.Count;
        }
        
        private void AddGridRowIfNeeded(bool force = false)
        {
            if (!force && this.nextGridColumn >= this.currentGrid.ColumnDefinitions.Count)
            {
                force = true;
            }
            if (force)
            {
                this.currentGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(1f, GridUnitType.Star)
                });
                this.nextGridColumn = 0;
            }
        }

        private void AddSection(ConfigParameterV1 parameter)
        {
            AddGridIfNeeded();
            AddGridRowIfNeeded(true);

            var label = new Label()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0f, 0f, 0f, 0f),
                Content = parameter.Label
            };
            Grid.SetRow(label, this.currentGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(label, 0);
            Grid.SetColumnSpan(label, this.currentGrid.ColumnDefinitions.Count);
            this.currentGrid.Children.Add(label);
            this.nextGridColumn = this.currentGrid.ColumnDefinitions.Count;
        }

        private void AddLabelIfSpace(string content)
        {
            if (!String.IsNullOrEmpty(content) && this.nextGridColumn < this.currentGrid.ColumnDefinitions.Count - 1)
            {
                var label = new Label()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0f, -3f, 0f, 0f),
                    Content = content
                };
                Grid.SetRow(label, this.currentGrid.RowDefinitions.Count - 1);
                Grid.SetColumn(label, this.nextGridColumn++);
                this.currentGrid.Children.Add(label);
            }
        }

        private void AddControlParameter(ConfigParameterV1 parameter)
        {
            void AddElement(UIElement element)
            {
                AddGridIfNeeded();
                AddGridRowIfNeeded();
                if (parameter.Type != ConfigParameterType.Text)
                {
                    AddLabelIfSpace(parameter.Label);
                }

                Grid.SetRow(element, this.currentGrid.RowDefinitions.Count - 1);
                Grid.SetColumn(element, this.nextGridColumn++);
                this.currentGrid.Children.Add(element);
            }

            switch (parameter.Type)
            {
                case ConfigParameterType.String:
                    { 
                        var textBox = new TextBox()
                        {
                            Height = 23f,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0f, 0f, 0f, 7f)
                        };
                        AddElement(textBox);

                        if (String.IsNullOrEmpty(parameter.Name))
                        {
                            textBox.IsReadOnly = true;
                            textBox.IsEnabled = false;
                        }
                        else
                        {
                            if (this.dataContext.ContainsKey(parameter.Name) && this.dataContext[parameter.Name] is string)
                            {
                                textBox.Text = this.dataContext[parameter.Name] as string;
                            }
                            textBox.TextChanged += (sender, e) => SetControlValue(parameter.Name, textBox.Text);
                        }
                        break;
                    }
                case ConfigParameterType.Boolean:
                    {
                        var checkBox = new CheckBox()
                        {
                            Height = 23f,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0f, 3f, 0f, 7f)
                        };
                        AddElement(checkBox);

                        if (String.IsNullOrEmpty(parameter.Name))
                        {
                            checkBox.IsEnabled = false;
                        }
                        else
                        {
                            if (this.dataContext.ContainsKey(parameter.Name) && this.dataContext[parameter.Name] is bool)
                            {
                                checkBox.IsChecked = (bool)this.dataContext[parameter.Name];
                            }
                            checkBox.Checked += (sender, e) => SetControlValue(parameter.Name, checkBox.IsChecked);
                        }

                        break;
                    }
                case ConfigParameterType.Integer:
                    {
                        var textBox = new IntegerTextBox()
                        {
                            Height = 23f,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0f, 0f, 0f, 7f),
                            MinValue = parameter.IntMinValue,
                            MaxValue = parameter.IntMaxValue,
                        };
                        AddElement(textBox);

                        if (String.IsNullOrEmpty(parameter.Name))
                        {
                            textBox.IsReadOnly = true;
                            textBox.IsEnabled = false;
                        }
                        else
                        {
                            if (this.dataContext.ContainsKey(parameter.Name) && this.dataContext[parameter.Name] is int)
                            {
                                textBox.Value = (int)this.dataContext[parameter.Name];
                            }
                            textBox.TextChanged += (sender, e) => SetControlValue(parameter.Name, textBox.Value);
                        }
                        break;
                    }
                case ConfigParameterType.Number:
                    {
                        var textBox = new DoubleTextBox()
                        {
                            Height = 23f,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0f, 0f, 0f, 7f),
                            MinValue = parameter.MinValue,
                            MaxValue = parameter.MaxValue,
                        };
                        AddElement(textBox);

                        if (String.IsNullOrEmpty(parameter.Name))
                        {
                            textBox.IsReadOnly = true;
                            textBox.IsEnabled = false;
                        }
                        else
                        {
                            if (this.dataContext.ContainsKey(parameter.Name) && this.dataContext[parameter.Name] is double)
                            {
                                textBox.Value = (double)this.dataContext[parameter.Name];
                            }
                            textBox.TextChanged += (sender, e) => SetControlValue(parameter.Name, textBox.Value);
                        }
                        break;
                    }
                case ConfigParameterType.Text:
                    {
                        var textBlock = new TextBlock()
                        {
                            Height = 23f,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0f, 2f, 0f, 7f)
                        };
                        AddElement(textBlock);
                        textBlock.Text = parameter.Label;
                        break;
                    }
                default:
                    return;
            }
        }
    }
}
