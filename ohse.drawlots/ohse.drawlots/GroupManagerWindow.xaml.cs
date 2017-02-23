using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// GroupManagerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GroupManagerWindow : Window
    {
        private int cid => ((@class) classList.SelectedItem).cid;
        private student[] students;
        private ObservableCollection<student> localStudents;

        public GroupManagerWindow()
        {
            InitializeComponent();
        }

        private void GroupManagerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            classList.ItemsSource = S.DB.@class.Local;
        }

        private void ClassList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddBtn.IsEnabled = true;
            students = S.DB.student.Local.Where(student => student.cid == cid && student.gid == -1).ToArray();
            localStudents = new ObservableCollection<student>(students);
            studentList.ItemsSource = localStudents;
            groupPanel.Children.Clear();
            foreach (var g in S.DB.group.Local.Where(g => g.cid == cid))
            {
                AddGroupPanel(g);
            }
        }

        private void AddGroupPanel(@group g)
        {
            var gp = new GroupPanelItem(g);
            gp.RemoveGroup += (o, students1) =>
            {
                foreach (var student in students1)
                {
                    student.gid = -1;
                    localStudents.Add(student);
                }
                groupPanel.Children.Remove((GroupPanelItem)o);
            };
            groupPanel.Children.Add(gp);
        }

        private void StudentList_OnDrop(object sender, DragEventArgs e)
        {
            string f = e.Data.GetFormats().First();
            var data = e.Data.GetData(f);
            if (data is student) // single
            {
                UpdateInfo(data);
            }
            else if (data is IEnumerable<student>)
            {
                foreach (var s in (IEnumerable<student>)data)
                {
                    UpdateInfo(s);
                }
            }
        }

        private void UpdateInfo(object data)
        {
            if (data is student)
            {
                ((student)data).gid = -1;
            }
        }

        private void GroupManagerWindow_OnClosed(object sender, EventArgs e)
        {
            S.DB.SaveChanges();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            int k = S.DB.group.Where(group => group.cid == cid).Select(group => group.gid).Max();
            var g = new @group()
            {
                cid = cid,
                gid = S.DB.group.Where(group => group.cid == cid).Select(group => group.gid).Max() + 1,
                name = "새로운 조"
            };
            S.DB.group.Add(g);
            S.DB.SaveChanges();
            AddGroupPanel(g);
            
        }
    }
}
