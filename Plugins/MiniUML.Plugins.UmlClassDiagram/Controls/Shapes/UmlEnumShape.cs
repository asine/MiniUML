using System.Windows.Controls;

namespace MiniUML.Plugins.UmlClassDiagram.Resources.Shapes
{
    public class UmlEnumShape : GenericUmlContainerShape
    {
        public UmlEnumShape()
        {
            createContextMenu();
        }

        private void createContextMenu()
        {
            AddMenuItem("Add _Member", "AddMember");
            ContextMenu.Items.Add(new Separator());
            AddMenuItem("Delete", "Delete");
            AddZOrderMenuItems();
        }
    }
}