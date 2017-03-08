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
    public class StudentHistory
    {
        public int sid { get; set; }
        public int num { get; set; }
        public string name { get; set; }
        public int count { get; set; }
    }

    /// <summary>
    /// HistoryManagerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HistoryManagerWindow : Window
    {
        private int cid => ((@class2)classList.SelectedItem).cid;
        private int sid;
        private StudentHistory[] shs;

        public HistoryManagerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            classList.ItemsSource = S.DB.@class.Local;
        }

        private void ClassList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStudent();
        }

        private void StudentGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is StudentHistory)
            {
                sid = ((StudentHistory) e.AddedItems[0]).sid;
                LoadHistory();
            }
        }

        private void LoadStudent()
        {
            var students = S.DB.student.Local.Where(student => student.cid == cid).OrderBy(student => student.num).ToArray();
            shs = students.Select(student => new StudentHistory()
            {
                sid = student.sid,
                name = student.name,
                num = student.num,
                count = S.DB.history.Count(history => history.sid == student.sid)
            }).ToArray();
            studentGrid.ItemsSource = shs;
        }

        private void LoadHistory()
        {
            historyGrid.ItemsSource =
                    S.DB.history.Local.Where(history => history.sid == sid).ToArray();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var history = (sender as Button).DataContext as history2;
            S.DB.history.Remove(history);
            S.DB.SaveChanges();
            LoadStudent();
            LoadHistory();
        }
    }
}
