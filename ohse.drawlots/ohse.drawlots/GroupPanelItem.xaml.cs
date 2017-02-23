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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ohse.drawlots
{
    /// <summary>
    /// GroupPanelItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GroupPanelItem : UserControl
    {
        private @group g;
        public EventHandler<student[]> RemoveGroup;

        public GroupPanelItem()
        {
            InitializeComponent();
        }

        public GroupPanelItem(@group g)
        {
            InitializeComponent();
            this.g = g;
            nameTxtBox.Text = g.name;
            var students = S.DB.student.Local.Where(student => student.cid == g.cid && student.gid == g.gid).ToArray();
            listBox.ItemsSource = new ObservableCollection<student>(students);
        }

        private void ListBox_OnDrop(object sender, DragEventArgs e)
        {
            string f = e.Data.GetFormats().First();
            var data = e.Data.GetData(f);
            if (data is student) // single
            {
                UpdateInfo(data);
            } else if (data is IEnumerable<student>)
            {
                foreach (var s in (IEnumerable<student> )data)
                {
                    UpdateInfo(s);
                }
            }

        }

        private void UpdateInfo(object data)
        {
            if (data is student)
            {
                ((student) data).gid = g.gid;
            }
        }

        private void RemoveClick(object sender, RoutedEventArgs e)
        {
            S.DB.group.Remove(g);
            RemoveGroup?.Invoke(this, listBox.ItemsSource.Cast<student>().ToArray());
        }

        private void NameTxtBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            g.name = nameTxtBox.Text;
        }
    }
}
