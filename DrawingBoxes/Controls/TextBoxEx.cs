using System.Windows.Controls;

namespace VaporDAW
{
    public class TextBoxEx : TextBox
    {
        private string previousText;
        private int previousCaretIndex;

        public double DoubleValue
        {
            get => this.doubleValue;
            set
            {
                this.doubleValue = value;
                this.Text = string.Format("{0:0.###}", value);
                this.previousText = this.Text;
                this.previousCaretIndex = this.CaretIndex;
            }
        }

        private double doubleValue = 0d;

        public TextBoxEx()
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
                this.doubleValue = 0;
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
            this.doubleValue = _v;
        }
    }
}
