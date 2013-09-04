using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;

namespace MiniUML.Plugins.UmlClassDiagram.Resources.Shapes
{
    /// <summary>
    /// Interaction logic for GenericUmlMemberShape.xaml
    /// </summary>
    public partial class GenericUmlMemberShape : Control
    {
        public GenericUmlMemberShape()
        {
            InitializeComponent();
            this.ContextMenu = createContextMenu();
        }

        private ContextMenu createContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem;

            menuItem = new MenuItem() { Header = "Delete", Tag = "Delete" };
            menuItem.Click += menuItem_Click;
            contextMenu.Items.Add(menuItem);

            return contextMenu;
        }

        private void menuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            XElement root = DataContext as XElement;

            switch ((string)menuItem.Tag)
            {
                case "Delete":
                    root.Remove();
                    break;
            }

            ListBox listBox = this.Template.FindName("_listBox", this) as ListBox;
            if (listBox != null) listBox.Focus();
        }

        private void content_LostFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject obj = sender as DependencyObject;
            do
            {
                ListBox listBox = obj as ListBox;
                if (listBox != null)
                {
                    CollectionView cv = listBox.ItemsSource as CollectionView;
                    cv.Refresh();
                    return;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
            while (obj != null);
        }
    }
}
