using System;
using System.Windows;
using System.Windows.Data;

namespace MiniUML.Diagnostics
{
    /// <summary>
    /// Interaction logic for ExceptionDialogWindow.xaml
    /// </summary>
    public partial class ExceptionDialogWindow : Window
    {
        public ExceptionDialogWindow(bool killApp)
        {
            InitializeComponent();
            this.Title = Application.Current.Resources["ApplicationName"] + " - Errors";

            _killApp = killApp;

            if (_killApp)
                button.Content = "Close program";
        }

        private void _button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_killApp)
                Environment.Exit(1);
        }

        bool _killApp;
    }

    public class ExceptionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Exception ex = value as Exception;

            if (ex != null)
                return ex.GetType().Name;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}