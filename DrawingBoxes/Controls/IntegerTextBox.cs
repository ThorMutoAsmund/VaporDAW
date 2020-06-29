using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VaporDAW
{
    public class IntegerTextBox : TextBox
    {
        private static readonly Regex regex = new Regex("[^0-9]+");

        public int Value
        {
            get => Int32.TryParse(this.Text, out int value) ? value : 0;
            set => this.Text = value.ToString();
        }        
        public int? MaxValue { get; set; }
        public int? MinValue { get; set; }

        public IntegerTextBox()
        {
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            FocusManager.SetIsFocusScope(this, true);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (this.MinValue.HasValue && this.Value < this.MinValue)
            {
                this.Value = this.MinValue.Value;
            }
            else if (this.MaxValue.HasValue && this.Value > this.MaxValue)
            {
                this.Value = this.MaxValue.Value;
            }

            FocusManager.SetIsFocusScope(this, false);
        }
    }
}
