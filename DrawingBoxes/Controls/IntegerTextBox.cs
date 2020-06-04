using System;
using System.Text.RegularExpressions;
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

        public IntegerTextBox()
        {
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
