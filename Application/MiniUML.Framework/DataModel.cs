using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace MiniUML.Framework
{
    /// <summary>
    /// Base class for data models. All public methods must be called on the instantiating thread only.
    /// </summary>
    public abstract class DataModel : DispatcherObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Possible states for a DataModel.
        /// </summary>
        public enum ModelState
        {
            Invalid,    // The model is in an invalid state
            Loading,    // The model is loading data
            Saving,     // The model is saving data
            Ready,      // The model is ready
            Busy        // The model is busy (currently in the middle of being updated)
        }

        /// <summary>
        /// PropertyChanged event for INotifyPropertyChanged implementation.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                VerifyAccess();
                _propertyChangedEvent += value;
            }
            remove
            {
                VerifyAccess();
                _propertyChangedEvent -= value;
            }
        }

        /// <summary>
        /// Gets or sets current state of the model.
        /// </summary>
        public ModelState State
        {
            get
            {
                VerifyAccess();
                return _state;
            }

            set
            {
                VerifyAccess();

                // Ensure that the state cannot be set to Ready while the model is invalid.
                if (value == ModelState.Ready && !IsValid())
                    value = ModelState.Invalid;

                if (value != _state)
                {
                    _state = value;
                    SendPropertyChanged("State");
                }
            }
        }

        /// <summary>
        /// Utility method for use by subclasses to notify that a property has changed.
        /// </summary>
        /// <param name="propertyName">The names of the properties.</param>
        protected void SendPropertyChanged(params string[] propertyNames)
        {
            VerifyAccess();
            if (_propertyChangedEvent != null)
            {
                foreach (string propertyName in propertyNames)
                    _propertyChangedEvent(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Determines if the model is valid. Override to provide custom behavior.
        /// Do not call the base version when overriding.
        /// </summary>
        /// <returns>True if the model is valid, otherwise false.</returns>
        protected virtual bool IsValid() { return true; }

        /// <summary>
        /// Verifies that the model is in an acceptable state.
        /// Throws InvalidOperationException if validation fails.
        /// </summary>
        /// <param name="acceptedStates">The acceptable states.</param>
        [DebuggerNonUserCode]
        protected void VerifyState(params ModelState[] acceptedStates)
        {
            foreach (ModelState s in acceptedStates)
                if (State == s) return; // OK.

            String msg = "The model state must be ";
            for (int i = 0; i < acceptedStates.Length; i++)
            {
                if (i > 0) msg += (i == acceptedStates.Length - 1 ? " or " : ", ");
                msg += acceptedStates[i];
            }

            throw new InvalidOperationException(msg);
        }

        private ModelState _state = ModelState.Invalid;
        private PropertyChangedEventHandler _propertyChangedEvent;
    }
}
