using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MiniUML.Framework
{
    /// <summary>
    /// Model for a command
    /// </summary>
    public abstract class CommandModel
    {
        #region Constructors

        public CommandModel()
        {
            _routedCommand = new RoutedCommand();
        }

        public CommandModel(RoutedUICommand command)
        {
            _routedCommand = command;
            _name = command.Text;
        }

        #endregion

        #region Properties

        /// <summary>
        /// RoutedCommand associated with the model.
        /// </summary>
        public RoutedCommand Command
        {
            get { return _routedCommand; }
        }

        /// <summary>
        /// Name of the command.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Description of the command.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Image representing the command.
        /// </summary>
        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines if the command is enabled. Override to provide custom behavior.
        /// Do not call the base version when overriding.
        /// </summary>
        public virtual void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        public abstract void OnExecute(object sender, ExecutedRoutedEventArgs e);

        #endregion

        private readonly RoutedCommand _routedCommand;
        private string _name;
        private string _description;
        private BitmapImage _image;
    }

    public interface IDragableCommandModel
    {
        void OnDragDropExecute(Point dropPoint);
    }
}