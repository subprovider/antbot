using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ktds.AntBot.Studio
{
    // ActivityDesigner1.xaml에 대한 상호 작용 논리
    public partial class ActivityDesigner1
    {
        public ActivityDesigner1()
        {
            InitializeComponent();
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Hide();

            MessageBox.Show("hide");

            Application.Current.MainWindow.Show();

        }
    }
}
