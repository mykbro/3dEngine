using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace _3dGraphics.Windows
{
    /// <summary>
    /// Logica di interazione per ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        public ConsoleWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void WriteLine(object o)
        {
            _console.Text += o.ToString() + '\n';
        }

        private void Clear()
        {
            _console.Text = string.Empty;
        }
    }
}
