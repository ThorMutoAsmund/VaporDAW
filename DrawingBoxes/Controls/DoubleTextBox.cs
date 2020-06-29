using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VaporDAW
{
    public class DoubleTextBox : TextBox
    {
        public string Format { get; set; } = "0.0000";

        private string previousText;
        private int previousCaretIndex;

        public double Value
        {
            get => this.value;
            set
            {
                this.value = value;
                this.Text = value.ToString(this.Format, System.Globalization.NumberFormatInfo.InvariantInfo);
                this.previousText = this.Text;
                this.previousCaretIndex = this.CaretIndex;
            }
        }

        public double? MaxValue { get; set; }
        public double? MinValue { get; set; }

        private double value = 0d;

        public DoubleTextBox()
        {
            this.TextChanged += TextBoxEx_TextChanged;
            this.KeyUp += TextBoxEx_KeyUp;
        }

        private void TextBoxEx_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            this.previousCaretIndex = this.CaretIndex;
        }

        private void TextBoxEx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                this.value = 0;
                this.previousText = this.Text;
                return;
            }

            if (!double.TryParse(this.Text, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out var _v))
            {
                this.TextChanged -= TextBoxEx_TextChanged;
                this.Text = this.previousText;
                this.TextChanged += TextBoxEx_TextChanged;
                this.CaretIndex = this.previousCaretIndex;
                return;
            }
            this.previousText = this.Text;
            this.value = _v;
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
