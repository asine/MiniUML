using System.Windows.Controls;

namespace MiniUML.Plugins.UmlClassDiagram.Resources.Shapes
{
    public class UmlInterfaceShape : GenericUmlContainerShape
    {
        public UmlInterfaceShape()
        {
            createContextMenu();
        }

        private void createContextMenu()
        {
            AddMenuItem("Add _Method", "AddMethod");
            AddMenuItem("Add _Property", "AddProperty");
            AddMenuItem("Add _Event", "AddEvent");
            ContextMenu.Items.Add(new Separator());
            AddMenuItem("Delete", "Delete");
            AddZOrderMenuItems();
        }

    }
}
