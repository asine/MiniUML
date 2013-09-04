using System.ComponentModel;

namespace MiniUML.Framework
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Utility method for use by subclasses to notify that a property has changed.
        /// </summary>
        /// <param name="propertyName">The names of the properties.</param>
        protected void SendPropertyChanged(params string[] propertyNames)
        {
            if (PropertyChanged != null)
            {
                foreach (string propertyName in propertyNames)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
