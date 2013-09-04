using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace MiniUML.Diagnostics
{
    public class FatalException : Exception
    {
        public FatalException() : base("Fatal application error") { }
        public FatalException(string message) : base(message) { }
        public FatalException(string message, Exception inner) : base(message, inner) { }
    }

    public static class DebugUtilities
    {
        private static void PrintVisualTree(DependencyObject o, String indent)
        {
            Debug.WriteLine(o.ToString().Replace("System.Windows.Controls.", "SWC."));

            int count = VisualTreeHelper.GetChildrenCount(o);
            for (int i = 0; i < count; i++)
            {
                Debug.Write(indent + "+ ");
                PrintVisualTree(VisualTreeHelper.GetChild(o, i), indent + (i == count - 1 ? "   " : "|  "));
            }
        }

        [DebuggerNonUserCode]
        public static void Assert(bool expr)
        {
            Assert(expr, "Assertion failed");
        }

        [DebuggerNonUserCode]
        public static void Assert(bool expr, String message)
        {
            if (!expr) throw new FatalException(message);
        }
    }
}
