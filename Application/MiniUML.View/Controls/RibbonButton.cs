using System.Windows;

namespace MiniUML.View.Controls
{
    public class RibbonButton : CommandButton
    {
        static RibbonButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonButton), new FrameworkPropertyMetadata(typeof(RibbonButton)));
        }
    }
}
