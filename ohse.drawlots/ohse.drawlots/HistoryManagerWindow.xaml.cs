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
    /// HistoryManagerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HistoryManagerWindow : Window
    {
        public HistoryManagerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            System.Windows.Data.CollectionViewSource historyViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("historyViewSource")));
            // CollectionViewSource.Source 속성을 설정하여 데이터를 로드합니다.
            // historyViewSource.Source = [제네릭 데이터 소스]
        }
    }
}
