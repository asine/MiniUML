using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace MiniUML.Plugins.UmlClassDiagram.Resources.Shapes
{
    /// <summary>
    /// Interaction logic for UmlCommentShape.xaml
    /// </summary>
    public partial class UmlCommentShape : UserControl
    {
        public UmlCommentShape()
        {
            InitializeComponent();
            ContextMenu = new ContextMenu();
            createContextMenu();
        }

        private void createContextMenu()
        {
            AddMenuItem("Delete", "Delete");
            AddZOrderMenuItems();
        }

        protected void AddMenuItem(string text, string id)
        {
            MenuItem menuItem = new MenuItem() { Header = text, Tag = id };
            menuItem.Click += menuItem_Click;
            ContextMenu.Items.Add(menuItem);
        }

        protected void AddZOrderMenuItems()
        {
            ContextMenu.Items.Add(new Separator());
            AddMenuItem("BringToFront", "BringToFront");
            AddMenuItem("SendToBack", "SendToBack");
        }

        protected void menuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            XElement element = DataContext as XElement;
            XElement root = element.Parent;

            switch ((string)menuItem.Tag)
            {
                case "Delete":
                    element.Remove();
                    break;

                case "BringToFront":
                    element.Remove();
                    root.Add(element);
                    break;

                case "SendToBack":
                    element.Remove();
                    root.AddFirst(element);
                    break;
            }

            ListBox listBox = this.Template.FindName("_listBox", this) as ListBox;
            if (listBox != null) listBox.Focus();
        }
    }
}
