﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Media;
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
        private List<@group2> pickedGroups = new List<@group2>();
        SoundPlayer tick = new SoundPlayer(Properties.Resources.tick);
        SoundPlayer applause = new SoundPlayer(Properties.Resources.appluase);

        private int? cid
        {
            get
            {
                @class2 c = this.ClassComboBox.SelectedItem as @class2;
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
                    S.DB.@class.Local.Add(new @class2()
                    {
                        cid = S.DB.@class.Local.Count,
                        class1 = S.DB.@class.Local.Count + 1,
                        year = DateTime.Now.Year
                    });
                    for (int j = 0; j < 30; j++)
                    {
                        S.DB.student.Local.Add(new student2()
                        {
                            cid = S.DB.@class.Local.Count - 1,
                            sid = S.DB.student.Local.Count,
                            gid = j/5,
                            name = GetRandomName(),
                            num = j + 1
                        });
                        if(j%5 == 0)
                            S.DB.group.Local.Add(new @group2()
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
            string[] names = new[] {"이순신", "홍길동", "아무개"};
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
                return GetHistories().Exists(history => history.sid == ((student2)o).sid);
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

        private student2[] GetClassStudents() => S.DB.student.Where(student => student.cid == cid).ToArray();
        
        private @group2[] GetClassGroups() => S.DB.group.Where(group => group.cid == cid).ToArray();

        private void DrawLots(object sender, RoutedEventArgs e)
        {
            student2[] sPool = GetStudentPool();
            student2 s = sPool[rand.Next(sPool.Length)];

            ShowDynamicResults(sPool, s, new Action(() =>
            {
                S.DB.history.Add(new history2()
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
            @group2[] gPool = GetGroupPool();
            @group2 g = gPool[rand.Next(gPool.Length)];

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

        private void SelectStudentGroup(@group2 g)
        {
            student2[] ss = S.DB.student.Where(s => s.cid == cid && s.gid == g.gid).ToArray();
            var panel = NamePanel;
            List<object> objs = new List<object>();
            foreach (object obj in panel.Children)
            {
                if (obj is ToggleButton)
                {
                    if (((ToggleButton) obj).Tag is student2)
                    {
                        student2 s = ((ToggleButton) obj).Tag as student2;
                        ((ToggleButton) obj).IsChecked = ss.Contains(s);
                    }
                }
            }
        }

        private int duration = 0;
        private Timer t = null;

        private string GetName(object obj)
        {
            if (obj is student2)
            {
                student2 s = ((student2) obj);
                int cnt = S.DB.history.Count(history => history.sid == s.sid);
                return $"{s.num:D2}. {s.name} ({cnt})";
            }
            if (obj is @group2)
            {
                @group2 g = ((@group2) obj);
                return $"{g.name}";
            }
            return "";
        }

        private void ShowDynamicResults(object[] sPool, object rst, Action dbDelegate, Action callback)
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
                        tick.Play();
                        var s = sPool[rand.Next(sPool.Length)];
                        Result.Content = GetName(s);
                        Result.Foreground = PickBrush();
                    }));
                }
                else
                {
                    if (Result.Dispatcher.CheckAccess() == false)
                    {
                        Result.Dispatcher.Invoke(DispatcherPriority.Send, (Action) (() =>
                        {
                            applause.Play();
                            dbDelegate.Invoke();
                            Result.Content = GetName(rst);
                            Result.Foreground = Brushes.Black;
                            t.Dispose();
                            callback.Invoke();
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

        private student2[] GetStudentPool()
        {
            return GetObjectPool(NamePanel, GetClassStudents(), o => GetHistories().Exists(history => history.sid == ((student2)o).sid)).Cast<student2>().ToArray();
        }

        private @group2[] GetGroupPool()
        {
            return GetObjectPool(GroupPanel, GetClassGroups(), o => pickedGroups.Exists(g => g.Equals(o))).Cast<@group2>().ToArray();
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

            List<history2> histories = GetHistories();

            if (objs.TrueForAll(obj => existFunc(obj)) == false)
            {
                objs.RemoveAll(obj => existFunc(obj));
            }

            return objs;
        }

        private List<history2> GetHistories()
        {
            DateTime? lastTime = S.DB.forget.Local.LastOrDefault(forget => forget.cid == this.cid.Value)?.datetime;

            List<history2> histories = new List<history2>();
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
                if (cObj is student2)
                {
                    student s = (student) cObj;
                    var hl = GetHistories();
                    var h = hl.FirstOrDefault(history => history.sid == s.sid);
                    if (h != null) S.DB.history.Local.Remove(h);
                    S.DB.SaveChanges();
                    LoadStudents();
                }
                else if (cObj is @group2)
                {
                    pickedGroups.Remove((group2) cObj);
                    LoadGroups();
                }
            }
        }

        private void OpenStudentManagement(object sender, RoutedEventArgs e)
        {
            new LoginWindow() {nextWindow = new StudentManagerWindow()}.ShowDialog();
        }

        private void OpenGroupManagement(object sender, RoutedEventArgs e)
        {
            new LoginWindow() { nextWindow = new GroupManagerWindow() }.ShowDialog();
        }

        private void OpenHistoryManagement(object sender, RoutedEventArgs e)
        {
            new LoginWindow() { nextWindow = new HistoryManagerWindow() }.ShowDialog();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void NewStage(object sender, RoutedEventArgs e)
        {
            if (cid.HasValue == false) return;
            LoginWindow lw = new LoginWindow() {nextFunc = () =>
            {
                if (MessageBox.Show("새로운 판을 구성합니다. 기존의 발표 기록은 유지됩니다. 계속 진행하시겠습니까?", "주의", MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes)
                {
                    S.DB.forget.Add(new forget2()
                    {
                        cid = cid.Value,
                        datetime = DateTime.Now
                    });
                    S.DB.SaveChanges();
                    LoadStudents();
                }
                return true;
            }};
            lw.ShowDialog();
        }
    }
}
