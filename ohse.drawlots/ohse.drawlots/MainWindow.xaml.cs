using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ohse.drawlots
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        
        Random rand = new Random();
        private student cStudent = null;

        private int? cid
        {
            get
            {
                @class c = this.ClassComboBox.SelectedItem as @class;
                return c?.cid;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitDB();
            GenerateRandomData();
            InitClassComboBox();
        }

        private void InitDB()
        {
            S.DB.@class.Load();
            S.DB.forget.Load();
            S.DB.history.Load();
            S.DB.student.Load();
        }

        private void GenerateRandomData()
        {
            if (S.DB.@class.Local.Count == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    S.DB.@class.Local.Add(new @class()
                    {
                        cid = S.DB.@class.Local.Count,
                        class1 = S.DB.@class.Local.Count + 1,
                        year = 2017
                    });
                    for (int j = 0; j < 30; j++)
                    {
                        S.DB.student.Local.Add(new student()
                        {
                            cid = S.DB.@class.Local.Count - 1,
                            sid = S.DB.student.Local.Count,
                            name = GetRandomName(),
                            num = j + 1
                        });
                    }
                }
                S.DB.SaveChanges();
            }
        }

        private string GetRandomName()
        {
            string[] names = new[] {"이태민", "홍길동", "권지용", "오세림", "이규화", "이서화"};
            return names[rand.Next(names.Length)];
        }

        private void InitClassComboBox()
        {
            ClassComboBox.ItemsSource = S.DB.@class.Local;
        }

        private void ClassComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStudents();
        }

        private void LoadStudents()
        {
            NamePanel.Children.Clear();
            student[] students = GetClassStudent();
            List<history> histories = GetHistories();
            foreach (var s in students)
            {
                Brush b = Brushes.AliceBlue;
                if (histories.Exists(history => history.sid == s.sid))
                {
                    b = Brushes.Khaki;
                }
                var toggleBtn = new ToggleButton()
                {
                    Content = $"{s.num:D2}. {s.name}",
                    Tag = s,
                    Margin = new Thickness(10),
                    Padding = new Thickness(10),
                    Background = b,
                    ContextMenu = studentCM
                };
                toggleBtn.ContextMenuOpening += (sender, args) => cStudent = (student) toggleBtn.Tag;
                toggleBtn.ContextMenuClosing += (sender, args) => cStudent = null;
                NamePanel.Children.Add(toggleBtn);
            }
        }

        private student[] GetClassStudent()
        {
            student[] students = S.DB.student.Where(student => student.cid == cid).ToArray();
            return students;
        }

        private void DrawLots(object sender, RoutedEventArgs e)
        {
            student[] sPool = GetStudentPool();
            student s = sPool[rand.Next(sPool.Length)];
            S.DB.history.Add(new history()
            {
                sid = s.sid,
                cid = s.cid,
                date = DateTime.Now,
            });
            S.DB.SaveChanges();

            ShowDynamicName(sPool, s);
        }

        private int duration = 0;
        private Timer t = null;

        private void ShowDynamicName(student[] sPool, student student)
        {
            if(t != null)
                t.Dispose();
            duration = 0;
            t = new Timer(state =>
            {
                if (duration < 33)
                {
                    duration++;
                    Dispatcher.BeginInvoke((Action) (() =>
                    {
                        var s = sPool[rand.Next(sPool.Length)];
                        Result.Content = $"{s.num:D2}. {s.name}";
                        Result.Foreground = PickBrush();
                    }));
                }
                else
                {
                    if (Result.Dispatcher.CheckAccess() == false)
                    {
                        Result.Dispatcher.BeginInvoke(DispatcherPriority.Send, (Action) (() =>
                        {
                            Result.Content = $"{student.num:D2}. {student.name}";
                            Result.Foreground = Brushes.Black;
                            t.Dispose();
                            LoadStudents();
                        }));
                    }
                }
            }, null, 0, 33);
        }

        private Brush PickBrush()
        {
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);

            return result;
        }

        private student[] GetStudentPool()
        {
            List<student> students = new List<student>();
            foreach (object obj in NamePanel.Children)
            {
                if (obj is ToggleButton)
                {
                    if (((ToggleButton)obj).IsChecked == true)
                    {
                        students.Add((student)((ToggleButton)obj).Tag);
                    }
                }
            }
            if (students.Count == 0)
                students.AddRange(GetClassStudent());

            List<history> histories = GetHistories();

            if (students.TrueForAll(student => histories.Exists(history => history.sid == student.sid)) == false)
            {
                students.RemoveAll(student => histories.Exists(history => history.sid == student.sid));
            }

            return students.ToArray();
        }

        private List<history> GetHistories()
        {
            DateTime? lastTime = S.DB.forget.Local.LastOrDefault(forget => forget.cid == this.cid.Value)?.datetime;

            List<history> histories = new List<history>();
            if (lastTime != null)
            {
                histories = S.DB.history.Local.Where(history => history.date > lastTime && history.cid == cid.Value).ToList();
            }
            else
            {
                histories = S.DB.history.Local.Where(history => history.cid == cid.Value).ToList();
            }
            return histories;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (cStudent != null)
            {
                var hl = GetHistories();
                var h = hl.FirstOrDefault(history => history.sid == cStudent.sid);
                if (h != null) S.DB.history.Local.Remove(h);
                LoadStudents();
            }
        }

        private void OpenStudentManagement(object sender, RoutedEventArgs e)
        {
            LoginWindow lw = new LoginWindow() {nextWindow = new StudentManagerWindow()};
            lw.ShowDialog();
        }

        private void OpenHistoryManagement(object sender, RoutedEventArgs e)
        {
            
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
