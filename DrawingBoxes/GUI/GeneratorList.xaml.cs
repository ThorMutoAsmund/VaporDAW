using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VaporDAW
{
    /// <summary>
    /// Interaction logic for GeneratorList.xaml
    /// </summary>
    public partial class GeneratorList : UserControl
    {
        public object SelectedItem => this.listView.SelectedItem;
        public IEnumerable ItemsSource 
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(GeneratorList));
        
        public event EventHandler<object> ItemDoubleClicked;

        public GeneratorList()
        {
            InitializeComponent();
        }

        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.ItemDoubleClicked?.Invoke(this, this.listView.SelectedItem);
            }
        }        
    }
}
