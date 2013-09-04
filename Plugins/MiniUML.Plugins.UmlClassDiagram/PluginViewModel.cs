using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using MiniUML.Framework;
using MiniUML.Model.ViewModels;
using MiniUML.View.Views;

namespace MiniUML.Plugins.UmlClassDiagram
{
    public class PluginViewModel : ViewModel
    {
        public PluginViewModel(MainWindowViewModel windowViewModel)
        {
            // Store a reference to the parent view model.
            _WindowViewModel = windowViewModel;

            // Create the commands in this view model.
            _commandUtilities.InitializeCommands(this);
        }

        #region View models

        public MainWindowViewModel _WindowViewModel { get; private set; }

        #endregion

        #region Commands

        // Command properties
        public CommandModel cmd_CreateInterfaceShape { get; private set; }
        public CommandModel cmd_CreateAbstractClassShape { get; private set; }
        public CommandModel cmd_CreateClassShape { get; private set; }
        public CommandModel cmd_CreateStructShape { get; private set; }
        public CommandModel cmd_CreateEnumShape { get; private set; }
        public CommandModel cmd_CreateAssociationShape { get; private set; }
        public CommandModel cmd_CreateInheritanceShape { get; private set; }
        public CommandModel cmd_CreateCommentShape { get; private set; }

        #region Command implementations

        private CommandUtilities _commandUtilities = new CommandUtilities();

        /// <summary>
        /// Utility class used by the command implementations.
        /// </summary>
        private class CommandUtilities
        {
            public void InitializeCommands(PluginViewModel viewModel)
            {
                viewModel.cmd_CreateInterfaceShape = new CreateInterfaceShapeCommandModel(viewModel);
                viewModel.cmd_CreateAbstractClassShape = new CreateAbstractClassShapeCommandModel(viewModel);
                viewModel.cmd_CreateClassShape = new CreateClassShapeCommandModel(viewModel);
                viewModel.cmd_CreateStructShape = new CreateStructShapeCommandModel(viewModel);
                viewModel.cmd_CreateEnumShape = new CreateEnumShapeCommandModel(viewModel);
                viewModel.cmd_CreateAssociationShape = new CreateAssociationShapeCommandModel(viewModel);
                viewModel.cmd_CreateInheritanceShape = new CreateInheritanceShapeCommandModel(viewModel);
                viewModel.cmd_CreateCommentShape = new CreateCommentShapeCommandModel(viewModel);
            }

            public abstract class AbstractCreateAssociationMouseHandler : ICanvasViewMouseHandler
            {
                private bool isDone = false;
                private Boolean hasBeenAdded = false;
                private XElement association;
                private DocumentViewModel viewModel;

                public AbstractCreateAssociationMouseHandler(DocumentViewModel viewModel, XElement association)
                {
                    this.viewModel = viewModel;
                    this.association = association;
                    association.Add(new XElement("Anchor", new XAttribute("Left", "0"), new XAttribute("Top", "0")));
                    association.Add(new XElement("Anchor", new XAttribute("Left", "0"), new XAttribute("Top", "0")));
                }

                public void OnShapeClick(XElement shape)
                {
                    if (isDone) return;

                    viewModel.vm_CanvasViewModel.CancelCanvasViewMouseHandler();
                }

                public void OnShapeDragBegin(Point position, XElement shape)
                {
                    if (isDone) return;

                    if (shape == null || !IsValidFrom(shape))
                    {
                        viewModel.vm_CanvasViewModel.CancelCanvasViewMouseHandler();
                        return;
                    }

                    String idString = shape.GetStringAttribute("Id");
                    if (idString == "")
                    {
                        idString = viewModel.dm_DocumentDataModel.GetUniqueId();
                        shape.SetAttributeValue("Id", idString);
                    }

                    association.SetAttributeValue("From", idString);
                    ((XElement)association.FirstNode).SetPositionAttributes(position);

                    // Add the shape to the document root.
                    association = viewModel.dm_DocumentDataModel.AddShape(association);
                    hasBeenAdded = true;

                    ((CanvasView)viewModel.v_CanvasView).PresenterFromElement(association).IsHitTestVisible = false;
                }

                public void OnShapeDragUpdate(Point position, Vector delta)
                {
                    if (isDone) return;

                    ((XElement)association.LastNode).SetPositionAttributes(position);
                }

                public void OnShapeDragEnd(Point position, XElement shape)
                {
                    if (isDone) return;

                    if (shape == null || !IsValidTo(shape))
                    {
                        viewModel.vm_CanvasViewModel.CancelCanvasViewMouseHandler();
                        return;
                    }

                    string idString = shape.GetStringAttribute("Id");
                    if (idString == "")
                    {
                        idString = viewModel.dm_DocumentDataModel.GetUniqueId();
                        shape.SetAttributeValue("Id", idString);
                    }

                    // HACK: Work around for odd not-quite-updated binding problem: Remove, then re-add.
                    association.Remove();
                    association.SetAttributeValue("To", idString);
                    association = viewModel.dm_DocumentDataModel.AddShape(association);

                    ((CanvasView)viewModel.v_CanvasView).PresenterFromElement(association).IsHitTestVisible = true;

                    cleanUp();
                    viewModel.vm_CanvasViewModel.FinishCanvasViewMouseHandler();
                }

                public void OnCancelMouseHandler()
                {
                    if (hasBeenAdded) association.Remove();
                    cleanUp();
                }

                private void cleanUp()
                {
                    viewModel.v_CanvasView.ForceCursor = true;
                    viewModel.v_CanvasView.Cursor = null;
                    isDone = true;
                }

                protected abstract bool IsValidFrom(XElement element);

                protected abstract bool IsValidTo(XElement element);
            }
        }

        /// <summary>
        /// Private implementation of the CreateInterfaceShape command.
        /// </summary>
        private class CreateInterfaceShapeCommandModel : CommandModel, IDragableCommandModel
        {
            public CreateInterfaceShapeCommandModel(PluginViewModel viewModel)
            {
                _viewModel = viewModel;
                this.Name = "Interface";
                this.Description = "Create a new Interface shape";
                this.Image = new BitmapImage(new Uri("/MiniUML.Plugins.UmlClassDiagram;component/Resources/Images/Command.CreateInterfaceShape.png", UriKind.Relative));
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _viewModel._WindowViewModel.vm_DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                OnDragDropExecute(new Point(10, 10));
            }

            public void OnDragDropExecute(Point dropPoint)
            {
                DocumentViewModel documentViewModel = _viewModel._WindowViewModel.vm_DocumentViewModel;

                XElement element = new XElement("Uml.Interface",
                                   new XAttribute("Name", "<name>"),
                                   new XAttribute("Description", "Interface"),
                                   new XAttribute("Top", dropPoint.Y),
                                   new XAttribute("Left", dropPoint.X));

                documentViewModel.dm_DocumentDataModel.DocumentRoot.Add(element);
            }

            private PluginViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the CreateAbstractClassShape command.
        /// </summary>
        private class CreateAbstractClassShapeCommandModel : CommandModel, IDragableCommandModel
        {
            public CreateAbstractClassShapeCommandModel(PluginViewModel viewModel)
            {
                _viewModel = viewModel;
                this.Name = "Abstract Class";
                this.Description = "Create a new Abstract Class shape";
                this.Image = new BitmapImage(new Uri("/MiniUML.Plugins.UmlClassDiagram;component/Resources/Images/Command.CreateAbstractClassShape.png", UriKind.Relative));
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _viewModel._WindowViewModel.vm_DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                OnDragDropExecute(new Point(10, 10));
            }

            public void OnDragDropExecute(Point dropPoint)
            {
                DocumentViewModel documentViewModel = _viewModel._WindowViewModel.vm_DocumentViewModel;

                XElement element = new XElement("Uml.AbstractClass",
                                   new XAttribute("Name", "<name>"),
                                   new XAttribute("Description", "Abstract Class"),
                                   new XAttribute("Top", dropPoint.Y),
                                   new XAttribute("Left", dropPoint.X));

                documentViewModel.dm_DocumentDataModel.DocumentRoot.Add(element);
            }

            private PluginViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the CreateClassShape command.
        /// </summary>
        private class CreateClassShapeCommandModel : CommandModel, IDragableCommandModel
        {
            public CreateClassShapeCommandModel(PluginViewModel viewModel)
            {
                _viewModel = viewModel;
                this.Name = "Class";
                this.Description = "Create a new Class shape";
                this.Image = new BitmapImage(new Uri("/MiniUML.Plugins.UmlClassDiagram;component/Resources/Images/Command.CreateClassShape.png", UriKind.Relative));
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _viewModel._WindowViewModel.vm_DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                OnDragDropExecute(new Point(10, 10));
            }

            public void OnDragDropExecute(Point dropPoint)
            {
                DocumentViewModel documentViewModel = _viewModel._WindowViewModel.vm_DocumentViewModel;

                XElement element = new XElement("Uml.Class",
                                   new XAttribute("Name", "<name>"),
                                   new XAttribute("Description", "Class"),
                                   new XAttribute("Top", dropPoint.Y),
                                   new XAttribute("Left", dropPoint.X));

                documentViewModel.dm_DocumentDataModel.DocumentRoot.Add(element);
            }

            private PluginViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the CreateStructShape command.
        /// </summary>
        private class CreateStructShapeCommandModel : CommandModel, IDragableCommandModel
        {
            public CreateStructShapeCommandModel(PluginViewModel viewModel)
            {
                _viewModel = viewModel;
                this.Name = "Struct";
                this.Description = "Create a new Struct shape";
                this.Image = new BitmapImage(new Uri("/MiniUML.Plugins.UmlClassDiagram;component/Resources/Images/Command.CreateStructShape.png", UriKind.Relative));
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _viewModel._WindowViewModel.vm_DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                OnDragDropExecute(new Point(10, 10));
            }

            public void OnDragDropExecute(Point dropPoint)
            {
                DocumentViewModel documentViewModel = _viewModel._WindowViewModel.vm_DocumentViewModel;

                XElement element = new XElement("Uml.Struct",
                                   new XAttribute("Name", "<name>"),
                                   new XAttribute("Description", "Struct"),
                                   new XAttribute("Top", dropPoint.Y),
                                   new XAttribute("Left", dropPoint.X));

                documentViewModel.dm_DocumentDataModel.DocumentRoot.Add(element);
            }

            private PluginViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the CreateEnumShape command.
        /// </summary>
        private class CreateEnumShapeCommandModel : CommandModel, IDragableCommandModel
        {
            public CreateEnumShapeCommandModel(PluginViewModel viewModel)
            {
                _viewModel = viewModel;
                this.Name = "Enum";
                this.Description = "Create a new Enum shape";
                this.Image = new BitmapImage(new Uri("/MiniUML.Plugins.UmlClassDiagram;component/Resources/Images/Command.CreateEnumShape.png", UriKind.Relative));
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _viewModel._WindowViewModel.vm_DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                OnDragDropExecute(new Point(10, 10));
            }

            public void OnDragDropExecute(Point dropPoint)
            {
                DocumentViewModel documentViewModel = _viewModel._WindowViewModel.vm_DocumentViewModel;

                XElement element = new XElement("Uml.Enum",
                                   new XAttribute("Name", "<name>"),
                                   new XAttribute("Description", "Enum"),
                                   new XAttribute("Top", dropPoint.Y),
                                   new XAttribute("Left", dropPoint.X));

                documentViewModel.dm_DocumentDataModel.DocumentRoot.Add(element);
            }

            private PluginViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the CreateAssociationShape command.
        /// </summary>
        private class CreateAssociationShapeCommandModel : CommandModel
        {
            public CreateAssociationShapeCommandModel(PluginViewModel viewModel)
            {
                _viewModel = viewModel;
                this.Name = "Association";
                this.Description = "Create a new association";
                this.Image = new BitmapImage(new Uri("/MiniUML.Plugins.UmlClassDiagram;component/Resources/Images/Command.CreateAssociationShape.png", UriKind.Relative));
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _viewModel._WindowViewModel.vm_DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                var model = _viewModel._WindowViewModel.vm_DocumentViewModel;

                model.v_CanvasView.ForceCursor = true;
                model.v_CanvasView.Cursor = Cursors.Pen;

                model.vm_CanvasViewModel.BeginCanvasViewMouseHandler(
                    new CreateAssociationMouseHandler(model, new XElement("Uml.Associations.Association")));
            }

            private PluginViewModel _viewModel;

            public class CreateAssociationMouseHandler : CommandUtilities.AbstractCreateAssociationMouseHandler
            {
                private XElement _fromElement;

                public CreateAssociationMouseHandler(DocumentViewModel viewModel, XElement association)
                    : base(viewModel, association) { }

                protected override bool IsValidFrom(XElement element)
                {
                    string[] validShapes = { "Uml.Interface", "Uml.Class", "Uml.AbstractClass", "Uml.Struct" };

                    _fromElement = element;

                    foreach (string s in validShapes)
                        if (element.Name == s) return true;

                    return false;
                }

                protected override bool IsValidTo(XElement element)
                {
                    string[] validShapes = { "Uml.Interface", "Uml.Class", "Uml.AbstractClass", "Uml.Struct", "Uml.Enum" };

                    foreach (string s in validShapes)
                        if (element.Name == s) return true;

                    return false;
                }
            }
        }

        /// <summary>
        /// Private implementation of the CreateAssociationShape command.
        /// </summary>
        private class CreateInheritanceShapeCommandModel : CommandModel
        {
            public CreateInheritanceShapeCommandModel(PluginViewModel viewModel)
            {
                _viewModel = viewModel;
                this.Name = "Inheritance association";
                this.Description = "Create a new inheritance association";
                this.Image = new BitmapImage(new Uri("/MiniUML.Plugins.UmlClassDiagram;component/Resources/Images/Command.CreateInheritanceShape.png", UriKind.Relative));
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _viewModel._WindowViewModel.vm_DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                var model = _viewModel._WindowViewModel.vm_DocumentViewModel;

                model.v_CanvasView.ForceCursor = true;
                model.v_CanvasView.Cursor = Cursors.Pen;

                model.vm_CanvasViewModel.BeginCanvasViewMouseHandler(
                    new CreateInheritanceMouseHandler(model, new XElement("Uml.Associations.Inheritance")));
            }

            private PluginViewModel _viewModel;

            public class CreateInheritanceMouseHandler : CommandUtilities.AbstractCreateAssociationMouseHandler
            {
                private XElement _fromElement;

                public CreateInheritanceMouseHandler(DocumentViewModel viewModel, XElement association)
                    : base(viewModel, association) { }

                protected override bool IsValidFrom(XElement element)
                {
                    string[] validShapes = { "Uml.Interface", "Uml.Class", "Uml.AbstractClass" };

                    _fromElement = element;

                    foreach (string s in validShapes)
                        if (element.Name == s) return true;

                    return false;
                }

                protected override bool IsValidTo(XElement element)
                {
                    string[] validShapes;

                    if (_fromElement.Name == "Uml.Interface")
                        validShapes = new string[] { "Uml.Interface" };
                    else
                        validShapes = new string[] { "Uml.Interface", "Uml.Class", "Uml.AbstractClass" };

                    foreach (string s in validShapes)
                        if (element.Name == s) return true;

                    return false;
                }
            }
        }

        /// <summary>
        /// Private implementation of the CreateCommentShape command.
        /// </summary>
        private class CreateCommentShapeCommandModel : CommandModel, IDragableCommandModel
        {
            public CreateCommentShapeCommandModel(PluginViewModel viewModel)
            {
                _viewModel = viewModel;
                this.Name = "Comment";
                this.Description = "Create a new Comment shape";
                this.Image = new BitmapImage(new Uri("/MiniUML.Plugins.UmlClassDiagram;component/Resources/Images/Command.CreateCommentShape.png", UriKind.Relative));
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _viewModel._WindowViewModel.vm_DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                OnDragDropExecute(new Point(10, 10));
            }

            public void OnDragDropExecute(Point dropPoint)
            {
                DocumentViewModel documentViewModel = _viewModel._WindowViewModel.vm_DocumentViewModel;

                XElement element = new XElement("Uml.Comment",
                                   new XAttribute("Text", "<comment>"),
                                   new XAttribute("Top", dropPoint.Y),
                                   new XAttribute("Left", dropPoint.X));

                documentViewModel.dm_DocumentDataModel.DocumentRoot.Add(element);
            }

            private PluginViewModel _viewModel;
        }

        #endregion

        #endregion
    }
}
