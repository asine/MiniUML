using System.Windows.Controls;
using MiniUML.View.Utilities;

namespace MiniUML.Plugins.Home.Controls.Shapes
{
    public partial class Text : UserControl
    {
        public Text()
        {
            InitializeComponent();
            ((ShapeIdToControlConverter)Resources["ShapeIdToControlConverter"]).ReferenceControl = this;
        }
    }
}
