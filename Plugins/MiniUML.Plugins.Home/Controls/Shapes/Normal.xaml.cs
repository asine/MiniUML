using System.Windows.Controls;
using MiniUML.View.Utilities;

namespace MiniUML.Plugins.Home.Controls.Shapes
{
    /// <summary>
    /// Interaction logic for Normal.xaml
    /// </summary>
    public partial class Normal : UserControl
    {
        public Normal()
        {
            InitializeComponent();
            ((ShapeIdToControlConverter)Resources["ShapeIdToControlConverter"]).ReferenceControl = this;
        }
    }
}
