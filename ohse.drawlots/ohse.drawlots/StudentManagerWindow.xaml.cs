using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ohse.drawlots
{
    /// <summary>
    ///     StudentManagerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StudentManagerWindow : Window
    {
        private bool addFlag;

        public StudentManagerWindow()
        {
            InitializeComponent();
        }

        private int cid
        {
            get
            {
                if (dgClass.SelectedItem is @class)
                    return ((@class) dgClass.SelectedItem).cid;
                else
                    return -1;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dgClass.ItemsSource = S.DB.@class.Local;
        }

        private void DgClass_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addFlag)
                if (
                    MessageBox.Show("수정된 학생 명부를 저장하고 계속 진행하시겠습니까? 아니요를 누를 경우 작업한 내역이 유실됩니다.", "경고",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Save();
            var c = dgClass.SelectedItem as @class;
            if (c != null)
            {
                dgStudent.ItemsSource =
                    new ObservableCollection<student>(S.DB.student.Local.Where(student => student.cid == c.cid));
                addFlag = false;
            }
        }

        private void DgClass_OnAddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            var c = e.NewItem as @class;
            if (c != null)
            {
                var lc = S.DB.@class.LastOrDefault();
                if (lc == null)
                    c.cid = 0;
                else
                    c.cid = lc.cid + 1;
                c.year = GetLastYear();
            }
        }

        private void Save()
        {
            var cs = GetClasses();
            var rCS = S.DB.@class.Local.Where(c => cs.Any(lc => lc.cid == c.cid) == false).ToArray();
            var aCS = cs.Where(c => S.DB.@class.Local.Any(dbC => dbC.cid == c.cid) == false).ToArray();
            S.DB.@class.RemoveRange(rCS);
            S.DB.@class.AddRange(aCS);

            var ss = GetStudents();
            var rSS =
                S.DB.student.Local.Where(
                    s => (s.cid == cid) && (ss.Any(ls => (ls.cid == s.cid) && (ls.sid == s.sid)) == false)).ToArray();
            var aSS =
                ss.Where(s => S.DB.student.Local.Any(dbS => (dbS.cid == s.cid) && (dbS.sid == s.sid)) == false)
                    .ToArray();
            S.DB.student.RemoveRange(rSS);
            S.DB.student.AddRange(aSS);

            S.DB.SaveChanges();
        }

        private @class[] GetClasses()
        {
            var cs = new List<@class>();
            foreach (@class c in dgClass.Items.SourceCollection)
                cs.Add(c);
            return cs.ToArray();
        }

        private student[] GetStudents()
        {
            var ss = new List<student>();
            foreach (student s in dgStudent.Items.SourceCollection)
                ss.Add(s);
            return ss.ToArray();
        }

        private void DgStudent_OnAddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            addFlag = true;
            e.NewItem = new student();
            var s = e.NewItem as student;
            var lastStudent = GetLastStudent();
            if (s != null)
            {
                var lastDBStudent = S.DB.student.OrderByDescending(student => student.sid).First();
                if (lastStudent != null)
                {
                    s.num = lastStudent.num + 1;
                    s.sid = lastStudent.sid + 1;
                    s.cid = lastStudent.cid;
                }
                else
                {
                    s.num = 1;
                    if (lastDBStudent != null)
                        s.sid = lastDBStudent.sid + 1;
                    s.cid = cid;
                }
            }
        }

        private student GetLastStudent()
        {
            var mNum = -1;
            student lastStudent = null;
            foreach (student s in dgStudent.Items.SourceCollection)
                if (s.num > mNum)
                {
                    mNum = s.num;
                    lastStudent = s;
                }
            return lastStudent;
        }

        private int GetLastYear()
        {
            var mNum = -1;
            foreach (@class s in dgClass.Items.SourceCollection)
                if (s.year > mNum)
                    mNum = s.year;
            return mNum;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
            Close();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
    }
}