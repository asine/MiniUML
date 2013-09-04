using System;
using System.Windows.Input;
using System.Xml.Linq;
using MiniUML.Framework;

namespace MiniUML.Model.ViewModels
{
    public class XmlViewModel : ViewModel
    {
        public XmlViewModel(DocumentViewModel documentViewModel)
        {
            // Store a reference to the parent view model.
            _DocumentViewModel = documentViewModel;

            // Create the commands in this view model.
            _commandUtilities.InitializeCommands(this);
        }

        public bool prop_DocumentChanged
        {
            get { return _documentChanged; }
            set
            {
                _documentChanged = value;
                base.SendPropertyChanged("prop_DocumentChanged");
            }
        }

        private bool _documentChanged;

        #region View models

        public DocumentViewModel _DocumentViewModel { get; private set; }

        #endregion

        #region Commands

        // Command properties
        public CommandModel cmd_UpdateDesigner { get; private set; }

        private CommandUtilities _commandUtilities = new CommandUtilities();

        /// <summary>
        /// Utility class used by the command implementations.
        /// </summary>
        private class CommandUtilities
        {
            public void InitializeCommands(XmlViewModel viewModel)
            {
                viewModel.cmd_UpdateDesigner = new UpdateDesignerCommandModel(viewModel);
            }
        }

        #region Command implementations

        /// <summary>
        /// Private implementation of the New command.
        /// </summary>
        private class UpdateDesignerCommandModel : CommandModel
        {
            public UpdateDesignerCommandModel(XmlViewModel viewModel)
            {
                _viewModel = viewModel;
                this.Name = "Update Designer";
                this.Description = "Update the design surface to reflect changes made to the underlying XML document.";
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = (_viewModel._DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready ||
                                _viewModel._DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Invalid) &&
                                _viewModel.prop_DocumentChanged;

                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                try
                {
                    _viewModel._DocumentViewModel.dm_DocumentDataModel.DocumentRoot = XElement.Parse((string)e.Parameter);
                }
                catch (Exception)
                {
                    _viewModel._DocumentViewModel.dm_DocumentDataModel.State = DataModel.ModelState.Invalid;
                    _viewModel._DocumentViewModel.dm_DocumentDataModel.DocumentRoot = new XElement("InvalidDocument");
                }
            }

            private XmlViewModel _viewModel;
        }

        #endregion

        #endregion
    }
}
