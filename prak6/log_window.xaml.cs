using System;
using System.Collections.Generic;
using System.IO;
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

namespace prak6
{
    /// <summary>
    /// Interaction logic for log_window.xaml
    /// </summary>
    public partial class log_window : Window

    {
        public log_window()
        {

            InitializeComponent();
            Title = "Логи";
            if (!File.Exists(Serl.PATH))
            {
                Box.ItemsSource = null;
            }

            else
            {
                string line;
                var file = new StreamReader(Serl.PATH);
                while ((line = file.ReadLine()) != null)
                {
                    Box.Items.Add(line);
                }
            }
        }
    }
}