using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xml.Linq;
using Microsoft.Win32;
using MiniUML.Diagnostics;
using MiniUML.Framework;
using MiniUML.Model.DataModels;

namespace MiniUML.Model.ViewModels
{
    public delegate bool LoadThemeAssemblyDelegate(String assemblyFilename);

    public class DocumentViewModel : ViewModel
    {
        /// <summary>
        /// Stores a reference to the app-global LoadThemeAssembly method.
        /// </summary>
        public static LoadThemeAssemblyDelegate LoadThemeAssemblyDelegate { get; set; }

        public DocumentViewModel(MainWindowViewModel windowViewModel)
        {
            // Store a reference to the parent view model.
            _WindowViewModel = windowViewModel;

            // Create and initialize the data model.
            _dataModel = new DocumentDataModel();
            _dataModel.New((Size)SettingsManager.Settings["DefaultPageSize"], (Thickness)SettingsManager.Settings["DefaultPageMargins"]);
            _dataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Suggest to WPF to refresh commands when the DocumentDataModel changes state.
                if (e.PropertyName == "State") CommandManager.InvalidateRequerySuggested();
            };

            // Create the commands in this view model.
            _commandUtility = new CommandUtility(this);

            // Create the view models.
            vm_CanvasViewModel = new CanvasViewModel(this);
            vm_XmlViewModel = new XmlViewModel(this);
        }

        /// <summary>
        /// Queries the user to save unsaved changes
        /// </summary>
        /// <returns>False on cancel, otherwise true.</returns>
        public bool QuerySaveChanges()
        {
            if (!dm_DocumentDataModel.HasUnsavedData) return true;

            MessageBoxResult result = MessageBox.Show(Application.Current.MainWindow, "Do you want to save changes to " + prop_DocumentFileName + "?",
                (string)Application.Current.Resources["ApplicationName"],
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    return ((SaveCommandModel)cmd_Save).Execute();

                case MessageBoxResult.No:
                    return true;
                
                default:
                    return false;
            }
        }

        public void LoadFile(String filename)
        {
            _dataModel.Load(filename);
            prop_DocumentFilePath = filename;
            vm_CanvasViewModel.prop_SelectedShapes.Clear();
        }

        public string prop_DocumentFilePath
        {
            get { return _filePath; }

            private set
            {
                _filePath = value;

                // If the current document is associated with a file, show the file name as part of the window title.
                if (!string.IsNullOrEmpty(_filePath))
                    _fileName = System.IO.Path.GetFileName(_filePath);
                else
                    _fileName = "Untitled";

                base.SendPropertyChanged("prop_DocumentFilePath", "prop_DocumentFileName");
            }
        }

        public string prop_DocumentFileName
        {
            get { return _fileName; }
        }

        public FrameworkElement v_CanvasView
        {
            get { return _designSurface; }
            set
            {
                _designSurface = value;
                base.SendPropertyChanged("v_CanvasView");
            }
        }

        private string _fileName = "Untitled";
        private string _filePath = null;
        private FrameworkElement _designSurface;
        private readonly DocumentDataModel _dataModel;

        #region Data models

        public DocumentDataModel dm_DocumentDataModel
        {
            get { return _dataModel; }
        }

        #endregion

        #region View models

        public MainWindowViewModel _WindowViewModel { get; private set; }
        public CanvasViewModel vm_CanvasViewModel { get; private set; }
        public XmlViewModel vm_XmlViewModel { get; private set; }

        #endregion

        #region Commands

        // Command properties
        public CommandModel cmd_New { get; private set; }
        public CommandModel cmd_Open { get; private set; }
        public CommandModel cmd_Save { get; private set; }
        public CommandModel cmd_SaveAs { get; private set; }
        public CommandModel cmd_Export { get; private set; }
        public CommandModel cmd_Print { get; private set; }
        public CommandModel cmd_Undo { get; private set; }
        public CommandModel cmd_Redo { get; private set; }
        public CommandModel cmd_SelectTheme { get; private set; }
            

        private CommandUtility _commandUtility;

        #region Command implementations

        /// <summary>
        /// Utility class used by the command implementations.
        /// </summary>
        private class CommandUtility
        {
            public CommandUtility(DocumentViewModel viewModel)
            {
                _viewModel = viewModel;

                // Initialize commands.
                viewModel.cmd_New = new NewCommandModel(viewModel);
                viewModel.cmd_Open = new OpenCommandModel(viewModel);
                viewModel.cmd_Save = new SaveCommandModel(viewModel);
                viewModel.cmd_SaveAs = new SaveAsCommandModel(viewModel);
                viewModel.cmd_Export = new ExportCommandModel(viewModel);
                viewModel.cmd_Print = new PrintCommandModel(viewModel);
                viewModel.cmd_Undo = new UndoCommandModel(viewModel);
                viewModel.cmd_Redo = new RedoCommandModel(viewModel);
                viewModel.cmd_SelectTheme = new SelectThemeCommandModel(viewModel);
            }

            public Rectangle GetDocumentRectangle()
            {
                FrameworkElement canvasView =  _viewModel.v_CanvasView;

                // Create a VisualBrush representing the contents of the  document
                // and use it to paint a rectangle of the same size as the page.
                Rectangle rect = new Rectangle()
                {
                    Width = canvasView.ActualWidth,
                    Height = canvasView.ActualHeight,
                    Fill = new VisualBrush(canvasView) { TileMode = TileMode.None }
                };

                // Measure and arrange.
                rect.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                rect.Arrange(new Rect(new Size(canvasView.ActualWidth, canvasView.ActualHeight)));

                return rect;
            }

            public Rectangle GetDocumentRectangle(Size desiredSize)
            {
                FrameworkElement canvasView = _viewModel.v_CanvasView;

                // Create a VisualBrush representing the contents of the  document.
                VisualBrush v = new VisualBrush(canvasView) { TileMode = TileMode.None };

                // Scale the brush to fit within the specified bounds.
                if (canvasView.ActualWidth > desiredSize.Width || canvasView.ActualHeight > desiredSize.Height)
                    v.Stretch = Stretch.Uniform;
                else
                    v.Stretch = Stretch.None;

                // Use the brush to paint a rectangle of the specified size.
                Rectangle rect = new Rectangle()
                {
                    Width = desiredSize.Width,
                    Height = desiredSize.Height,
                    Fill = v
                };

                // Measure and arrange.
                rect.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                rect.Arrange(new Rect(rect.DesiredSize));

                return rect;
            }

            public PrintTicket PrintTicket;
            public PrintQueue PrintQueue;

            private DocumentViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the New command.
        /// </summary>
        private class NewCommandModel : CommandModel
        {
            public NewCommandModel(DocumentViewModel viewModel)
                : base(ApplicationCommands.New)
            {
                _viewModel = viewModel;
                this.Description = "Create new document.";
                this.Image = (BitmapImage)Application.Current.Resources["Style.Images.Commands.New"];
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = (_viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready ||
                                _viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Invalid);

                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                // Save changes before closing current document?
                if (!_viewModel.QuerySaveChanges()) return;
                
                // Get size and margins of the current document, measured in device independent units.
                XElement documentRoot = _viewModel.dm_DocumentDataModel.DocumentRoot;
                Size currentPageSize = new Size(
                    documentRoot.GetDoubleAttribute("PageWidth", ((Size)SettingsManager.Settings["DefaultPageSize"]).Width),
                    documentRoot.GetDoubleAttribute("PageHeight", ((Size)SettingsManager.Settings["DefaultPageSize"]).Height));
                Thickness currentPageMargins;
                try
                {
                    String pageMarginsString = documentRoot.GetStringAttribute("PageMargins");
                    currentPageMargins = (Thickness) (new ThicknessConverter()).ConvertFromInvariantString(pageMarginsString);
                }
                catch (Exception)
                {
                    currentPageMargins = (Thickness)SettingsManager.Settings["DefaultPageMargins"];
                }

                // Create NewDocumentWindow 
                IFactory newDocumentWindowFactory = Application.Current.Resources["NewDocumentWindowFactory"] as IFactory;
                Window newDocumentWindow = newDocumentWindowFactory.CreateObject() as Window;

                // Configure window.
                NewDocumentWindowViewModel newDocumentWindowViewModel = new NewDocumentWindowViewModel()
                {
                    prop_PageSize = currentPageSize,
                    prop_PageMargins = currentPageMargins
                };
                newDocumentWindow.DataContext = newDocumentWindowViewModel;
                newDocumentWindow.Owner = Application.Current.MainWindow;

                // Show window; return if canceled.
                if (!newDocumentWindow.ShowDialog().GetValueOrDefault()) return;

                // Create document.
                newDocumentWindow.DataContext = null;
                _viewModel.prop_DocumentFilePath = null;
                _viewModel.vm_CanvasViewModel.prop_SelectedShapes.Clear();
                _viewModel._dataModel.New(newDocumentWindowViewModel.prop_PageSize, newDocumentWindowViewModel.prop_PageMargins);
            }

            private DocumentViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the Open command.
        /// </summary>
        private class OpenCommandModel : CommandModel
        {
            public OpenCommandModel(DocumentViewModel viewModel)
                : base(ApplicationCommands.Open)
            {
                _viewModel = viewModel;
                this.Description = "Open an existing document.";
                this.Image = (BitmapImage)Application.Current.Resources["Style.Images.Commands.Open"];
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = (_viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready ||
                                _viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Invalid);

                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                // Save changes before closing current document?
                if (!_viewModel.QuerySaveChanges()) return;

                // Create and configure OpenFileDialog.
                FileDialog dlg = new OpenFileDialog()
                {
                    Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*",
                    DefaultExt = "xml",
                    AddExtension = true,
                    ValidateNames = true,
                    CheckFileExists = true
                };

                // Show dialog; return if canceled.
                if (!dlg.ShowDialog(Application.Current.MainWindow).GetValueOrDefault()) return;

                try
                {
                    // Open the document.
                    _viewModel.LoadFile(dlg.FileName);
                }
                catch (Exception ex)
                {
                    ExceptionManager.Register(ex,
                        "Operation aborted.",
                        "An error occured while opening the file " + dlg.FileName + ".");

                    ExceptionManager.ShowErrorDialog(true);
                }
            }

            private DocumentViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the Save command.
        /// </summary>
        private class SaveCommandModel : CommandModel
        {
            public SaveCommandModel(DocumentViewModel viewModel)
                : base(ApplicationCommands.Save)
            {
                _viewModel = viewModel;
                this.Description = "Save the current document.";
                this.Image = (BitmapImage)Application.Current.Resources["Style.Images.Commands.Save"];
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = (_viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready);
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                Execute();
            }

            public bool Execute()
            {
                string file = _viewModel.prop_DocumentFilePath;

                try
                {
                    if (File.Exists(file))
                    {
                        // Save document to the existing file.
                        _viewModel._dataModel.Save(file);
                        return true;
                    }
                    else
                    {
                        // Execute SaveAs command.
                        return ((SaveAsCommandModel)_viewModel.cmd_SaveAs).Execute();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.Register(ex,
                        "Operation aborted.",
                        "An error occured while saving to the file " + file + ".");

                    ExceptionManager.ShowErrorDialog(true);
                }

                return false;
            }

            private DocumentViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the Save As command.
        /// </summary>
        private class SaveAsCommandModel : CommandModel
        {
            public SaveAsCommandModel(DocumentViewModel viewModel)
                : base(ApplicationCommands.SaveAs)
            {
                _viewModel = viewModel;
                this.Description = "Save the current document to a new file.";
                this.Image = (BitmapImage)Application.Current.Resources["Style.Images.Commands.SaveAs"];
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = (_viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready);
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                Execute();
            }

            public bool Execute()
            {
                // Create and configure SaveFileDialog.
                FileDialog dlg = new SaveFileDialog()
                {
                    Filter = "XML Files (*.xml)|*.xml",
                    AddExtension = true,
                    ValidateNames = true
                };

                // Show dialog; return if canceled.
                if (!dlg.ShowDialog(Application.Current.MainWindow).GetValueOrDefault()) return false;

                try
                {
                    // Save document.
                    _viewModel._dataModel.Save(dlg.FileName);
                    _viewModel.prop_DocumentFilePath = dlg.FileName;

                    return true;
                }
                catch (Exception ex)
                {
                    ExceptionManager.Register(ex,
                        "Operation aborted.",
                        "An error occured while saving to the file " + dlg.FileName + ".");

                    ExceptionManager.ShowErrorDialog(true);
                }

                return false;
            }

            private DocumentViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the Export command.
        /// </summary>
        private class ExportCommandModel : CommandModel
        {
            public ExportCommandModel(DocumentViewModel viewModel)
            {
                _viewModel = viewModel;
                this.Name = "Export";
                this.Description = "Save the current document to a new file.";
                this.Image = (BitmapImage)Application.Current.Resources["Style.Images.Commands.Export"];
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = (_viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready);
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                // Create and configure SaveFileDialog.
                FileDialog dlg = new SaveFileDialog()
                {
                    ValidateNames = true,
                    AddExtension = true,
                    Filter = "PNG Image Files (*.png)|*.png|" +
                             "JPEG Image Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                             "Windows Bitmap Image Files (*.bmp)|*.bmp|" +
                             "XPS Document Files (*.xps)|*.xps"
                };

                // Show dialog; return if canceled.
                if (!dlg.ShowDialog(Application.Current.MainWindow).GetValueOrDefault()) return;

                try
                {
                    // Save document to a file of the specified type.
                    switch (dlg.FilterIndex)
                    {
                        case 1: saveAsBitmap(dlg.FileName, new PngBitmapEncoder(), true); break;
                        case 2: saveAsBitmap(dlg.FileName, new JpegBitmapEncoder(), false); break;
                        case 3: saveAsBitmap(dlg.FileName, new BmpBitmapEncoder(), false); break;
                        case 4: saveAsXPS(dlg.FileName); break;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.Register(ex,
                        "Save operation aborted.",
                        "An error occured while saving the document to " + dlg.FileName + ".");

                    ExceptionManager.ShowErrorDialog(true);
                }
            }

            private void saveAsXPS(string file)
            {
                // Deselect shapes while saving.
                List<XElement> selectedItems = new List<XElement>(_viewModel.vm_CanvasViewModel.prop_SelectedShapes);
                _viewModel.vm_CanvasViewModel.prop_SelectedShapes.Clear();

                // Get a rectangle representing the page.
                Rectangle page = _viewModel._commandUtility.GetDocumentRectangle();

                try
                {
                    using (Package package = Package.Open(file, FileMode.Create))
                    {
                        using (XpsDocument xpsDocument = new XpsDocument(package))
                        {
                            // Write the document.
                            XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                            xpsWriter.Write(page);
                        }
                    }
                }
                finally
                {
                    // Reselect shapes.
                    _viewModel.vm_CanvasViewModel.SelectShapes(selectedItems);
                }
            }

            private void saveAsBitmap(string file, BitmapEncoder encoder, bool enableTransparentBackground)
            {
                // Create and configure ExportDocumentWindowViewModel.
                ExportDocumentWindowViewModel windowViewModel = new ExportDocumentWindowViewModel()
                {
                    prop_Resolution = 96,
                    prop_EnableTransparentBackground = enableTransparentBackground,
                    prop_TransparentBackground = false
                };

                // Create and configure ExportDocumentWindow.
                IFactory windowFactory = Application.Current.Resources["ExportDocumentWindowFactory"] as IFactory;
                Window window = windowFactory.CreateObject() as Window;
                window.Owner = Application.Current.MainWindow;
                window.DataContext = windowViewModel;

                // Show window; return if canceled.
                if (!window.ShowDialog().GetValueOrDefault()) return;

                // Deselect shapes while saving.
                List<XElement> selectedItems = new List<XElement>(_viewModel.vm_CanvasViewModel.prop_SelectedShapes);
                _viewModel.vm_CanvasViewModel.prop_SelectedShapes.Clear();

                // Get a rectangle representing the page and wrap it in a border to allow a background color to be set.
                Border page = new Border() { Child = _viewModel._commandUtility.GetDocumentRectangle() };

                // Use transparent or white background?
                if (!windowViewModel.prop_TransparentBackground)
                    page.Background = Brushes.White;

                // Measure and arrange.
                page.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                page.Arrange(new Rect(page.DesiredSize));

                try
                {
                    // Save the document.
                    using (FileStream fs = new FileStream(file, FileMode.Create))
                    {
                        double scaleFactor = windowViewModel.prop_Resolution / 96;
                        RenderTargetBitmap bmp = new RenderTargetBitmap((int)(page.ActualWidth * scaleFactor), (int)(page.ActualHeight * scaleFactor),
                            windowViewModel.prop_Resolution, windowViewModel.prop_Resolution, PixelFormats.Pbgra32);
                        bmp.Render(page);
                        encoder.Frames.Add(BitmapFrame.Create(bmp));
                        encoder.Save(fs);
                    }
                }
                finally
                {
                    //Reselect shapes.
                    _viewModel.vm_CanvasViewModel.SelectShapes(selectedItems);
                }
            }

            private DocumentViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the Print command.
        /// </summary>
        private class PrintCommandModel : CommandModel
        {
            public PrintCommandModel(DocumentViewModel viewModel)
                : base(ApplicationCommands.Print)
            {
                _viewModel = viewModel;
                this.Description = "Print the current document.";
                this.Image = (BitmapImage)Application.Current.Resources["Style.Images.Commands.Print"];
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = (_viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready ||
                                _viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Saving);
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                // Create PrintDialog
                PrintDialog dlg = new PrintDialog();

                // Get previously used PrintTicket.
                if (_viewModel._commandUtility.PrintTicket != null)
                    dlg.PrintTicket = _viewModel._commandUtility.PrintTicket;

                // Get previously used PrintQueue.
                if (_viewModel._commandUtility.PrintQueue != null)
                    dlg.PrintQueue = _viewModel._commandUtility.PrintQueue;

                // Show dialog; return if canceled.
                if (!dlg.ShowDialog().GetValueOrDefault()) return;

                // Store the PrintTicket and PrintQueue for later use.
                _viewModel._commandUtility.PrintTicket = dlg.PrintTicket;
                _viewModel._commandUtility.PrintQueue = dlg.PrintQueue;

                // Deselect shapes while printing.
                List<XElement> selectedItems = new List<XElement>(_viewModel.vm_CanvasViewModel.prop_SelectedShapes);
                _viewModel.vm_CanvasViewModel.prop_SelectedShapes.Clear();

                // Print the document.
                Rectangle page = _viewModel._commandUtility.GetDocumentRectangle(new Size(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight));
                dlg.PrintVisual(page, _viewModel.prop_DocumentFileName);

                // Reselect shapes.
                _viewModel.vm_CanvasViewModel.SelectShapes(selectedItems);
            }

            private DocumentViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the Undo command.
        /// </summary>
        private class UndoCommandModel : CommandModel
        {
            public UndoCommandModel(DocumentViewModel viewModel)
                : base(ApplicationCommands.Undo)
            {
                _viewModel = viewModel;
                this.Description = "Undo";
                this.Image = (BitmapImage)Application.Current.Resources["Style.Images.Commands.Undo"];
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = ((_viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready ||
                                _viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Invalid) &&
                                _viewModel._dataModel.HasUndoData);

                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _viewModel._dataModel.Undo();
            }

            private DocumentViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the Redo command.
        /// </summary>
        private class RedoCommandModel : CommandModel
        {
            public RedoCommandModel(DocumentViewModel viewModel)
                : base(ApplicationCommands.Redo)
            {
                _viewModel = viewModel;
                this.Description = "Redo";
                this.Image = (BitmapImage)Application.Current.Resources["Style.Images.Commands.Redo"];
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = ((_viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready ||
                                _viewModel.dm_DocumentDataModel.State == DataModel.ModelState.Invalid) &&
                                _viewModel._dataModel.HasRedoData);

                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _viewModel._dataModel.Redo();
            }

            private DocumentViewModel _viewModel;
        }

        /// <summary>
        /// Private implementation of the SelectTheme command.
        /// </summary>
        private class SelectThemeCommandModel : CommandModel
        {
            public SelectThemeCommandModel(DocumentViewModel viewModel)
                : base()
            {
                _viewModel = viewModel;
                this.Name = "Change";
                this.Description = "Change the color theme of the application.";
                this.Image = (BitmapImage)Application.Current.Resources["Style.Images.Commands.SelectTheme"];
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = true;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                // Create and configure OpenFileDialog.
                FileDialog dlg = new OpenFileDialog()
                {
                    Filter = "Plugin Files (MiniUML.Themes.*.dll)|MiniUML.Themes.*.dll",
                    DefaultExt = "dll",
                    AddExtension = true,
                    ValidateNames = true,
                    CheckFileExists = true,
                    InitialDirectory = System.IO.Path.GetDirectoryName((string) SettingsManager.Settings["ThemeAssembly"])
                };

                // Show dialog; return if canceled.
                if (!dlg.ShowDialog(Application.Current.MainWindow).GetValueOrDefault()) return;

                if (LoadThemeAssemblyDelegate(dlg.FileName))
                {
                    SettingsManager.Settings["ThemeAssembly"] = dlg.FileName;
                    SettingsManager.SaveSettings();
                }

                ExceptionManager.ShowErrorDialog(true); //TODO: We should actually do this after ALL commands...
            }

            private DocumentViewModel _viewModel;
        }

        #endregion

        #endregion
    }
}
