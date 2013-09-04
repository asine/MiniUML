using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MiniUML.Diagnostics
{
    public static class ExceptionManager
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Register(Exception ex)
        {
            ex.Data.Add("ExceptionManager.Timestamp", DateTime.Now.ToString());
            _exceptions.Push(ex);
            _hasNewExceptions = true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Register(Exception ex, string message)
        {
            ex.Data.Add("ExceptionManager.Message", message);
            Register(ex);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Register(Exception ex, string recoveryAction, string message)
        {
            ex.Data.Add("ExceptionManager.RecoveryAction", recoveryAction);
            Register(ex, message);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void RegisterCritical(Exception ex, string message)
        {
            ex.Data.Add("ExceptionManager.IsCritical", "True");
            Register(ex, "None", message);
            showErrorDialog(true);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ShowWarningDialog(string message)
        {
            MessageBox.Show(Application.Current.MainWindow, message,
                Application.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ShowErrorDialog(bool onlyIfChanged)
        {
            if (_hasNewExceptions || !onlyIfChanged)
                showErrorDialog(false);
        }

        private static void showErrorDialog(bool killApp)
        {
            Window owner = Application.Current.MainWindow;
            ExceptionDialogWindow dialog = new ExceptionDialogWindow(killApp);
            dialog.Owner = owner;

            dialog.ShowDialog();

            if (!_keepExceptions) { _exceptions.Clear(); }
            else
            {
                foreach (Exception ex in _exceptions)
                {
                    if (!ex.Data.Contains("ExceptionManager.HasBeenShown"))
                        ex.Data.Add("ExceptionManager.HasBeenShown", "True");
                }
            }
        }

        public static bool KeepExceptions
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _keepExceptions; }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _keepExceptions = value;
                if (!_keepExceptions) _exceptions.Clear();
            }
        }

        public static Stack<Exception> Exceptions
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _exceptions; }
        }

        private static bool _keepExceptions = true;
        private static bool _hasNewExceptions = false;
        private static Stack<Exception> _exceptions = new Stack<Exception>();
    }
}