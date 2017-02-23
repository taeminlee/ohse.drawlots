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
        private object cObj = null;
        private List<@group> pickedGroups = new List<@group>();

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
            S.DB.group.Load();
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
                            gid = j/5,
                            name = GetRandomName(),
                            num = j + 1
                        });
                        if(j%5 == 0)
                            S.DB.group.Local.Add(new @group()
                            {
                                cid = S.DB.@class.Local.Count - 1,
                                gid = j/5,
                                name = $"{(j / 5) + 1}조",
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
            LoadGroups();
        }

        private void LoadStudents()
        {
            LoadEntities(NamePanel, GetClassStudents(), (o) =>
            {
                return GetHistories().Exists(history => history.sid == ((student)o).sid);
            });
        }

        private void LoadGroups()
        {
            LoadEntities(GroupPanel, GetClassGroups(), (o) =>
            {
                return pickedGroups.Exists(g => g.Equals(o));
            });
        }

        private void LoadEntities(WrapPanel panel, object[] objs, Func<object, bool> existFunc)
        {
            panel.Children.Clear();
            foreach (var o in objs)
            {
                Brush b = Brushes.AliceBlue;
                if (existFunc(o))
                {
                    b = Brushes.Khaki;
                }
                var toggleBtn = new ToggleButton()
                {
                    Content = GetName(o),
                    Tag = o,
                    Margin = new Thickness(10),
                    Padding = new Thickness(10),
                    Background = b,
                    ContextMenu = studentCM,
                };
                
                toggleBtn.ContextMenuOpening += (sender, args) => cObj = toggleBtn.Tag;
                toggleBtn.ContextMenuClosing += (sender, args) => cObj = null;
                panel.Children.Add(toggleBtn);
            }
        }

        private student[] GetClassStudents() => S.DB.student.Where(student => student.cid == cid).ToArray();
        
        private @group[] GetClassGroups() => S.DB.group.Where(group => group.cid == cid).ToArray();

        private void DrawLots(object sender, RoutedEventArgs e)
        {
            student[] sPool = GetStudentPool();
            student s = sPool[rand.Next(sPool.Length)];

            ShowDynamicResults(sPool, s, new Action(() =>
            {
                S.DB.history.Add(new history()
                {
                    sid = s.sid,
                    cid = s.cid,
                    date = DateTime.Now,
                });
                S.DB.SaveChanges();
            }), new Action(LoadStudents));
        }

        private void SelGroup(object sender, RoutedEventArgs e)
        {
            @group[] gPool = GetGroupPool();
            @group g = gPool[rand.Next(gPool.Length)];

            ShowDynamicResults(gPool, g, new Action(() =>
            {
                if(pickedGroups.Contains(g) == false)
                    pickedGroups.Add(g);
            }), new Action(() =>
            {
                SelectStudentGroup(g);
                LoadGroups();
            }));
        }

        private void SelectStudentGroup(@group g)
        {
            student[] ss = S.DB.student.Where(s => s.cid == cid && s.gid == g.gid).ToArray();
            var panel = NamePanel;
            List<object> objs = new List<object>();
            foreach (object obj in panel.Children)
            {
                if (obj is ToggleButton)
                {
                    if (((ToggleButton) obj).Tag is student)
                    {
                        student s = ((ToggleButton) obj).Tag as student;
                        ((ToggleButton) obj).IsChecked = ss.Contains(s);
                    }
                }
            }
        }

        private int duration = 0;
        private Timer t = null;

        private string GetName(object obj)
        {
            if (obj is student)
            {
                student s = ((student) obj);
                return $"{s.num:D2}. {s.name}";
            }
            if (obj is @group)
            {
                @group g = ((@group) obj);
                return $"{g.name}";
            }
            return "";
        }

        private void ShowDynamicResults(object[] sPool, object rst, Delegate dbDelegate, Delegate callback)
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
                        Result.Content = GetName(s);
                        Result.Foreground = PickBrush();
                    }));
                }
                else
                {
                    if (Result.Dispatcher.CheckAccess() == false)
                    {
                        Result.Dispatcher.BeginInvoke(DispatcherPriority.Send, (Action) (() =>
                        {
                            dbDelegate.DynamicInvoke();
                            Result.Content = GetName(rst);
                            Result.Foreground = Brushes.Black;
                            t.Dispose();
                            callback.DynamicInvoke();
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
            return GetObjectPool(NamePanel, GetClassStudents(), o => GetHistories().Exists(history => history.sid == ((student)o).sid)).Cast<student>().ToArray();
        }

        private @group[] GetGroupPool()
        {
            return GetObjectPool(GroupPanel, GetClassGroups(), o => pickedGroups.Exists(g => g.Equals(o))).Cast<@group>().ToArray();
        }

        private List<object> GetObjectPool(WrapPanel panel, object[] classObjs, Func<object, bool> existFunc)
        {
            //List<student> students = new List<student>();
            List<object> objs = new List<object>();
            foreach (object obj in panel.Children)
            {
                if (obj is ToggleButton)
                {
                    if (((ToggleButton)obj).IsChecked == true)
                    {
                        objs.Add(((ToggleButton)obj).Tag);
                    }
                }
            }
            if (objs.Count == 0)
                objs.AddRange(classObjs);

            List<history> histories = GetHistories();

            if (objs.TrueForAll(obj => existFunc(obj)) == false)
            {
                objs.RemoveAll(obj => existFunc(obj));
            }

            return objs;
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
            if (cObj != null)
            {
                if (cObj is student)
                {
                    student s = (student) cObj;
                    var hl = GetHistories();
                    var h = hl.FirstOrDefault(history => history.sid == s.sid);
                    if (h != null) S.DB.history.Local.Remove(h);
                    LoadStudents();
                }
                else if (cObj is @group)
                {
                    pickedGroups.Remove((group) cObj);
                    LoadGroups();
                }
            }
        }

        private void OpenStudentManagement(object sender, RoutedEventArgs e)
        {
            LoginWindow lw = new LoginWindow() {nextWindow = new StudentManagerWindow()};
            lw.ShowDialog();
        }

        private void OpenGroupManagement(object sender, RoutedEventArgs e)
        {
            new GroupManagerWindow().ShowDialog();
        }

        private void OpenHistoryManagement(object sender, RoutedEventArgs e)
        {
            new HistoryManagerWindow().ShowDialog();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        

        
    }
}
