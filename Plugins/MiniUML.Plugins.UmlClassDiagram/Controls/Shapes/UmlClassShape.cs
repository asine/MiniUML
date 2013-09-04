using System.Windows.Controls;

namespace MiniUML.Plugins.UmlClassDiagram.Resources.Shapes
{
    public class UmlClassShape : GenericUmlContainerShape
    {
        public UmlClassShape()
        {
            createContextMenu();
        }

        private void createContextMenu()
        {
            AddMenuItem("Add _Method", "AddMethod");
            AddMenuItem("Add _Property", "AddProperty");
            AddMenuItem("Add _Field", "AddField");
            AddMenuItem("Add _Event", "AddEvent");
            AddMenuItem("Add _Class", "AddClass");
            ContextMenu.Items.Add(new Separator());
            AddMenuItem("Delete", "Delete");
            AddZOrderMenuItems();
        }
    }
}