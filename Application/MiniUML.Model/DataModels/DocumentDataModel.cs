using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using MiniUML.Diagnostics;
using MiniUML.Framework;

namespace MiniUML.Model.DataModels
{
    public class DocumentDataModel : DataModel
    {
        public const string RootElementName = "MiniUML_Document";
        
        /// <summary>
        /// Constructor initializing the data model as an empty document.
        /// </summary>
        public DocumentDataModel()
        {
            setDocumentRoot(new XElement("invalid"));

            base.State = ModelState.Invalid;
        }

        /// <summary>
        /// Create a new document.
        /// </summary>
        public void New(Size pageSize, Thickness pageMargins)
        {
            base.VerifyAccess();

            VerifyState(ModelState.Ready, ModelState.Invalid);

            setDocumentRoot(new XElement(RootElementName,
                new XAttribute("PageWidth", pageSize.Width),
                new XAttribute("PageHeight", pageSize.Height),
                new XAttribute("PageMargins", pageMargins.ToString())));
            clearUndoRedo();
            _maxId = 0;
            _hasUnsavedData = false;

            base.State = ModelState.Ready;

            base.SendPropertyChanged("DocumentRoot", "ObservableDocumentRoot", "HasUndoData", "HasRedoData", "HasUnsavedData");
        }

        /// <summary>
        /// Load document from a file.
        /// </summary>
        public void Load(string filename)
        {
            base.VerifyAccess();

            VerifyState(ModelState.Ready, ModelState.Invalid);

            ModelState fallbackState = base.State;
            base.State = ModelState.Loading;

            XElement newDocumentRoot;

            try
            {
                newDocumentRoot = XElement.Load(filename);

                if (!isValid(newDocumentRoot))
                    throw new Exception("Invalid document!");
           }
            catch
            {
                base.State = fallbackState;
                throw;
            }

            setDocumentRoot(newDocumentRoot);
            clearUndoRedo();
            _hasUnsavedData = false;
            _maxId = 0;

            base.State = ModelState.Ready;
            base.SendPropertyChanged("DocumentRoot", "ObservableDocumentRoot", "HasUndoData", "HasRedoData", "HasUnsavedData");
        }

        /// <summary>
        /// Save document to a file.
        /// </summary>
        public void Save(string filename)
        {
            base.VerifyAccess();

            VerifyState(ModelState.Ready);

            base.State = ModelState.Saving;

            try
            {
                _documentRoot.Save(filename);
                _hasUnsavedData = false;
            }
            finally
            {
                base.State = ModelState.Ready;
            }

            base.SendPropertyChanged("HasUnsavedData");
        }

        /// <summary>
        /// Gets a value indicating whether an Redo operation is possible.
        /// An INotifyPropertyChanged-enabled property.
        /// </summary>
        public bool HasUnsavedData
        {
            get
            {
                base.VerifyAccess();
                return (_hasUnsavedData);
            }
        }

        /// <summary>
        /// Roll back to the previous state.
        /// </summary>
        public void Undo()
        {
            base.VerifyAccess();

            VerifyState(ModelState.Ready, ModelState.Invalid);

            if (HasUndoData)
            {
                UndoState us = _undoList.First.Value;
                _redoList.AddFirst(new UndoState(_documentRoot, _hasUnsavedData));
                _undoList.RemoveFirst();
                setDocumentRoot(us.DocumentRoot);
                _hasUnsavedData = us.HasUnsavedData;

                base.State = ModelState.Ready;

                base.SendPropertyChanged("DocumentRoot", "ObservableDocumentRoot", "HasUndoData", "HasRedoData", "HasUnsavedData");
            }
        }

        /// <summary>
        /// Roll forward to the next state.
        /// </summary>
        public void Redo()
        {
            base.VerifyAccess();

            VerifyState(ModelState.Ready, ModelState.Invalid);

            if (HasRedoData)
            {
                UndoState us = _redoList.First.Value;
                _undoList.AddFirst(new UndoState(_documentRoot, _hasUnsavedData));
                _redoList.RemoveFirst();
                setDocumentRoot(us.DocumentRoot);
                _hasUnsavedData = us.HasUnsavedData;

                base.State = ModelState.Ready;

                base.SendPropertyChanged("DocumentRoot", "ObservableDocumentRoot", "HasUndoData", "HasRedoData", "HasUnsavedData");
            }
        }

        /// <summary>
        /// Gets a value indicating whether an Undo operation is possible.
        /// An INotifyPropertyChanged-enabled property.
        /// </summary>
        public bool HasUndoData
        {
            get
            {
                base.VerifyAccess();
                return (_undoList.Count > 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether an Redo operation is possible.
        /// An INotifyPropertyChanged-enabled property.
        /// </summary>
        public bool HasRedoData
        {
            get
            {
                base.VerifyAccess();
                return (_redoList.Count > 0);
            }
        }

        /// <summary>
        /// Gets the root XElement of the document. A PropertyChanged event is raised for this only when the root element is replaced, e.g. in New or Load.
        /// An INotifyPropertyChanged-enabled property.
        /// </summary>
        public XElement DocumentRoot
        {
            get
            {
                base.VerifyAccess();
                return _documentRoot;
            }

            set
            {
                base.VerifyAccess();

                if (value == null)
                    throw new ArgumentNullException();

                BeginOperation("Set DocumentRoot");
                setDocumentRoot(value);
                EndOperation("Set DocumentRoot");

                base.SendPropertyChanged("DocumentRoot", "ObservableDocumentRoot");
            }
        }

        /// <summary>
        /// Gets the root XElement of the document. Identical to DocumentRoot, except that
        /// a PropertyChanged event will be raised for this property whenever the document changes in any way.
        /// An INotifyPropertyChanged-enabled property.
        /// </summary>
        public XElement ObservableDocumentRoot
        {
            get { return DocumentRoot; }
            set { DocumentRoot = value; }
        }

        /// <summary>
        /// Begins an "atomic" operation, during which the model state is Busy and no restore points are created.
        /// </summary>
        public void BeginOperation(String operationName)
        {
            VerifyAccess();

            // Debug.WriteLine("Begin operation #" + (_operationLevel + 1) + ": " + operationName);

            if (_operationLevel++ == 0)
            {
                VerifyState(ModelState.Ready, ModelState.Invalid);

                setPendingUndoState();
                State = ModelState.Busy;
            }
            else VerifyState(ModelState.Busy);
        }

        public void EndOperation(String operationName)
        {
            EndOperationWithoutCreatingUndoState(operationName);

            if (State == ModelState.Ready) applyPendingUndoState();
        }

        public void EndOperationWithoutCreatingUndoState(String operationName)
        {
            DebugUtilities.Assert(_operationLevel > 0, "Trying to end operation " + operationName + " that hasn't begun");

            // Debug.WriteLine("End operation #" + _operationLevel + ": " + operationName);

            VerifyState(ModelState.Busy);
            if (--_operationLevel == 0) State = ModelState.Ready;
        }

        protected override bool IsValid()
        {
            return isValid(_documentRoot);
        }

        private bool isValid(XElement documentRoot)
        {
            //TODO: To do this right we should verify the entire document.
            if (documentRoot.Name != RootElementName) return false;
            if (!(documentRoot.GetDoubleAttribute("PageWidth", -1) > 0)) return false;
            if (!(documentRoot.GetDoubleAttribute("PageHeight", -1) > 0)) return false;
            return true;
        }

        private void setPendingUndoState()
        {
            VerifyAccess();

            VerifyState(ModelState.Ready, ModelState.Invalid);

            _pendingUndoState = new UndoState(_documentRoot, _hasUnsavedData);
        }

        private void applyPendingUndoState()
        {
            VerifyAccess();

            VerifyState(ModelState.Ready, ModelState.Invalid);

            if (_pendingUndoState.DocumentRootXml == _documentRoot.ToString())
                return; // Nothing has actually changed.

            if (_undoList.First == null || _undoList.First.Value.DocumentRootXml != _pendingUndoState.DocumentRootXml)
            {
                _undoList.AddFirst(_pendingUndoState);
                _redoList.Clear();
                _hasUnsavedData = true;

                base.SendPropertyChanged("HasUndoData", "HasRedoData", "HasUnsavedData");
            }
        }

        private void clearUndoRedo()
        {
            _undoList.Clear();
            _redoList.Clear();
        }

        private void setDocumentRoot(XElement newDocumentRoot)
        {
            if (_documentRoot != null)
            {
                _documentRoot.Changed -= documentRoot_Changed;
                _documentRoot.Changing -= documentRoot_Changing;
            }

            if (newDocumentRoot != null)
            {
                newDocumentRoot.Changed += documentRoot_Changed;
                newDocumentRoot.Changing += documentRoot_Changing;
            }

            _documentRoot = newDocumentRoot;
        }

        private void documentRoot_Changed(object sender, XObjectChangeEventArgs e)
        {
            if (State != ModelState.Busy)
            {
                if (!IsValid())
                {
                    if (State == ModelState.Ready) State = ModelState.Invalid;
                }
                else
                {
                    if (State == ModelState.Invalid) State = ModelState.Ready;
                }

                applyPendingUndoState();
            }

            base.SendPropertyChanged("ObservableDocumentRoot");
        }

        private void documentRoot_Changing(object sender, XObjectChangeEventArgs e)
        {
            if (State != ModelState.Busy) setPendingUndoState();
        }

        private int _operationLevel = 0;

        private bool _hasUnsavedData;
        private UndoState _pendingUndoState;
        private XElement _documentRoot;
        private LinkedList<UndoState> _undoList = new LinkedList<UndoState>();
        private LinkedList<UndoState> _redoList = new LinkedList<UndoState>();

        private struct UndoState
        {
            private readonly string _documentXml;
            private readonly bool _hasUnsavedData;

            public UndoState(XElement documentRoot, bool hasUnsavedData)
            {
                _documentXml = documentRoot.ToString();
                _hasUnsavedData = hasUnsavedData;
            }

            public bool HasUnsavedData { get { return _hasUnsavedData; } }
            public XElement DocumentRoot { get { return XElement.Parse(_documentXml); } }
            public string DocumentRootXml { get { return _documentXml; } }
        }

        #region Shape stuff

        public XElement GetShapeById(String id)
        {
            IEnumerable<XElement> shapes = from c in DocumentRoot.Elements() where c.GetStringAttribute("Id") == id select c;
            foreach (XElement result in shapes) return result;
            return null;
        }

        /// <summary>
        /// Returns a unique (unused) Id. The returned ids are guaranteed not to repeat unless New or Load is called.
        /// </summary>
        public String GetUniqueId()
        {
            const string prefix = "auto_";

            foreach (XElement element in DocumentRoot.Elements())
            {
                String elementIdString = element.GetStringAttribute("Id");
                if (!elementIdString.StartsWith(prefix)) continue;

                Int64 elementId;
                if (!Int64.TryParse(elementIdString.Substring(prefix.Length), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out elementId)) continue;

                if (_maxId <= elementId) _maxId = elementId + 1;
            }

            return prefix + _maxId.ToString("X", CultureInfo.InvariantCulture);
        }

        private Int64 _maxId = 0;

        
        /// <summary>
        /// An undocumented "feature" of XElement.Add is that it *SOMETIMES* (and only sometimes) adds a copy of the passed XElement, not the element itself.
        /// Nothing we can do about that. However, this method returns the created copy for further reference.
        /// Also assigns a unique ID to the shape, if the existing ID is taken.
        /// </summary>
        public XElement AddShape(XElement shape)
        {
            VerifyState(ModelState.Ready, ModelState.Busy);

            String idString = shape.GetStringAttribute("Id");
            if (idString != "" && GetShapeById(idString) != null)
                shape.SetAttributeValue("Id", GetUniqueId());

            DocumentRoot.Add(shape);
            XElement copy = (XElement)DocumentRoot.LastNode;
            return copy;
        }

        #endregion
    }
}
