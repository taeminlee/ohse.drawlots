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

namespace ohse.drawlots
{
    /// <summary>
    /// LoginWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window
    {
        public Window nextWindow { get; set; } = new MainWindow();

        public LoginWindow()
        {
            InitializeComponent();
            PW.Focus();
        }

        private void PW_OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                Login();
        }

        private void LoginClick(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void Login()
        {
            if (PW.Password == "tndjq1")
            {
                nextWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("암호를 다시 한번 확인해 주십시오.");
            }
        }
    }
}
