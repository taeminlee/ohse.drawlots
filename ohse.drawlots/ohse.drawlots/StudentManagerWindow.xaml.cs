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
        private List<student2> students;
        private List<@class2> classes;
        private Dictionary<@class2, ObservableCollection<student2>> classStudents = new Dictionary<@class2, ObservableCollection<student2>>();

        public StudentManagerWindow()
        {
            InitializeComponent();
            students = S.DB.student.ToList();
            classes = S.DB.@class.ToList();
            classes.ForEach(@class =>
            {
                classStudents.Add(@class, new ObservableCollection<student2>(students.Where(student => student.cid == @class.cid)));
            });
        }

        private int cid
        {
            get
            {
                if (dgClass.SelectedItem is @class2)
                    return ((@class2) dgClass.SelectedItem).cid;
                else
                    return -1;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dgClass.ItemsSource = classes;
        }

        private void DgClass_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = dgClass.SelectedItem as @class2;
            if (c != null)
            {
                if (classStudents.ContainsKey(c))
                {
                    dgStudent.ItemsSource = classStudents[c];
                    addFlag = false;
                }
            }
        }

        private void DgClass_OnAddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            e.NewItem = new @class2();
            var c = e.NewItem as @class2;
            if (c != null)
            {
                var lc = classes.LastOrDefault();
                if (lc == null)
                    c.cid = 0;
                else
                    c.cid = lc.cid + 1;
                c.year = GetLastYear();
                classStudents.Add(c, new ObservableCollection<student2>());
            }
        }

        private void Save()
        {
            var cs = classes;
            var rCS = S.DB.@class.Local.Where(c => cs.Any(lc => lc.cid == c.cid) == false).ToArray();
            var aCS = cs.Where(c => S.DB.@class.Local.Any(dbC => dbC.cid == c.cid) == false).ToArray();
            S.DB.@class.RemoveRange(rCS);
            S.DB.@class.AddRange(aCS);

            var ss = classStudents.SelectMany(pair => pair.Value);
            var rSS = S.DB.student.Local.Where(s => ss.Any(ls => ls.sid == s.sid) == false).ToArray();
            var aSS = ss.Where(s => S.DB.student.Local.Any(dbS => dbS.sid == s.sid) == false).ToArray();
            S.DB.student.RemoveRange(rSS);
            S.DB.student.AddRange(aSS);

            S.DB.SaveChanges();
        }

        private @class2[] GetClasses()
        {
            var cs = new List<@class2>();
            foreach (@class2 c in dgClass.Items.SourceCollection)
                cs.Add(c);
            return cs.ToArray();
        }

        private student2[] GetStudents()
        {
            var ss = new List<student2>();
            foreach (student2 s in dgStudent.Items.SourceCollection)
                ss.Add(s);
            return ss.ToArray();
        }

        private void DgStudent_OnAddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            addFlag = true;
            e.NewItem = new student2();
            var s = e.NewItem as student2;
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

        private student2 GetLastStudent()
        {
            var mNum = -1;
            student2 lastStudent = null;
            foreach (student2 s in dgStudent.Items.SourceCollection)
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
            foreach (@class2 s in dgClass.Items.SourceCollection)
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