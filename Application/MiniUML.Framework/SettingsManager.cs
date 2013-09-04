using System;
using System.Configuration;
using MiniUML.Diagnostics;

namespace MiniUML.Framework
{
    public static class SettingsManager
    {
        public static ApplicationSettingsBase Settings;

        public static void SaveSettings()
        {
            try
            {
                Settings.Save();
            }
            catch (Exception ex)
            {
                ExceptionManager.Register(ex, "The preferences were not saved properly.", "Could not save preferences.");
                ExceptionManager.ShowErrorDialog(false);
            }
        }
    }
}
