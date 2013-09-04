using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniUML.Framework;
using MiniUML.Model.DataModels;

namespace MiniUML.Model.Test
{
    /// <summary>
    ///This is a test class for DocumentDataModelTest and is intended
    ///to contain all DocumentDataModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DocumentDataModelTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [DebuggerNonUserCode]
        public void DocumentDataModelInvariant(DocumentDataModel ddm)
        {
            Assert.AreSame(ddm.DocumentRoot, ddm.ObservableDocumentRoot);
        }

        [DebuggerNonUserCode]
        public void AssertAreEqualStrings(object expect, object actual)
        {
            Assert.AreEqual(expect.ToString(), actual.ToString());
        }
        
        /// <summary>
        ///A test for DocumentDataModel Constructor, testing the invariant and that all properties are at set correctly.
        ///</summary>
        [TestMethod()]
        public void DocumentDataModelConstructorTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Invalid, target.State);
            AssertAreEqualStrings(new XElement("invalid"), target.DocumentRoot);
            Assert.IsFalse(target.HasUndoData);
            Assert.IsFalse(target.HasRedoData);
            Assert.IsFalse(target.HasUnsavedData);
        }

        /// <summary>
        ///A test for ObservableDocumentRoot
        ///</summary>
        [TestMethod()]
        public void ObservableDocumentRootTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            DocumentDataModelInvariant(target);
            XElement expected = new XElement(DocumentDataModel.RootElementName);
            XElement actual;
            target.ObservableDocumentRoot = expected;
            actual = target.ObservableDocumentRoot;
            AssertAreEqualStrings(expected, actual);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Invalid, target.State);
        }

        /// <summary>
        ///A test for HasUnsavedData
        ///</summary>
        [TestMethod()]
        public void HasUnsavedDataTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            target.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
            Assert.IsFalse(target.HasUnsavedData);

            target.AddShape(new XElement("TestTag"));
            Assert.IsTrue(target.HasUnsavedData);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            target.Undo(); //Undo should return the datamodel to a state with no unsaved data.
            Assert.IsFalse(target.HasUnsavedData);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
        }

        /// <summary>
        ///A test for HasUndoData
        ///</summary>
        [TestMethod()]
        public void HasUndoDataTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            target.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
            Assert.IsFalse(target.HasUndoData);

            target.AddShape(new XElement("TestTag"));
            Assert.IsTrue(target.HasUndoData);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            target.Undo();
            Assert.IsFalse(target.HasUndoData);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
        }

        /// <summary>
        ///A test for HasRedoData
        ///</summary>
        [TestMethod()]
        public void HasRedoDataTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            DocumentDataModelInvariant(target);
            target.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            //Test if the property is false at initialisation.
            Assert.IsFalse(target.HasRedoData);

            //Test if the property is true after an undo
            target.AddShape(new XElement("TestTag"));
            target.Undo();
            Assert.IsTrue(target.HasRedoData);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            //Test if the property is false after a redo.
            target.Redo();
            Assert.IsFalse(target.HasRedoData);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            //Test if redo is cleared if a new element is added after undo
            target.Undo();
            target.AddShape(new XElement("TestTag"));
            Assert.IsFalse(target.HasRedoData);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
        }

        /// <summary>
        ///A test for DocumentRoot
        ///</summary>
        [TestMethod()]
        public void DocumentRootTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            DocumentDataModelInvariant(target);

            XElement docRoot = new XElement("TestTag");
            target.DocumentRoot = docRoot;
            AssertAreEqualStrings(docRoot, target.DocumentRoot);

            try
            {
                target.DocumentRoot = null;
                Assert.Fail("DocumentRoot accepted null value");
            }
            catch (ArgumentNullException) { }
            AssertAreEqualStrings(docRoot, target.DocumentRoot);

            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Invalid, target.State);
        }

        /// <summary>
        ///A test for Undo
        ///</summary>
        [TestMethod()]
        public void UndoTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            target.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            XElement expected = new XElement(target.DocumentRoot);
            target.AddShape(new XElement("TestTag"));
            target.Undo();
            Assert.IsFalse(target.HasUndoData);
            AssertAreEqualStrings(expected, target.DocumentRoot);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
        }

        /// <summary>
        ///A test for Redo
        ///</summary>
        [TestMethod()]
        public void RedoTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            target.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            target.AddShape(new XElement("TestTag"));
            XElement expected = new XElement(target.DocumentRoot);
            target.Undo();
            target.Redo();
            Assert.IsFalse(target.HasRedoData);
            AssertAreEqualStrings(expected, target.DocumentRoot);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
        }

        /// <summary>
        ///A test for New
        ///</summary>
        [TestMethod()]
        public void NewTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Invalid, target.State);

            Size validPageSize = new Size(1, 1);
            Thickness validPageMargins = new Thickness(1, 1, 1, 1);
            target.New(validPageSize, validPageMargins);
            XElement validNewDocument = new XElement(DocumentDataModel.RootElementName,
                new XAttribute("PageWidth", validPageSize.Width),
                new XAttribute("PageHeight", validPageSize.Height),
                new XAttribute("PageMargins", validPageMargins.ToString()));
            AssertAreEqualStrings(validNewDocument, target.DocumentRoot);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
            Assert.IsFalse(target.HasUnsavedData);
            Assert.IsFalse(target.HasRedoData);
            Assert.IsFalse(target.HasUndoData);
            DocumentDataModelInvariant(target);

            Size invalidPageSize = new Size(double.NaN, double.NaN);
            target.New(invalidPageSize, validPageMargins);
            Assert.AreEqual(DataModel.ModelState.Invalid, target.State);
            Assert.IsFalse(target.HasUnsavedData);
            Assert.IsFalse(target.HasRedoData);
            Assert.IsFalse(target.HasUndoData);
            DocumentDataModelInvariant(target);
        }

        /// <summary>
        ///A test for GetUniqueId
        ///</summary>
        [TestMethod()]
        public void GetUniqueIdTest()
        {
            DocumentDataModel model = new DocumentDataModel();
            model.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(model);
            Assert.AreEqual(DataModel.ModelState.Ready, model.State);

            // The first unique id should be auto_0:
            string firstUniqueId = model.GetUniqueId();
            Assert.AreEqual("auto_0", firstUniqueId);
            Assert.AreSame(null, model.GetShapeById(firstUniqueId));

            // Adding an id which almost looks like an auto-id does not change GetUniqueId.
            XElement shape = model.AddShape(new XElement("Test", new XAttribute("Id", "auto_not")));
            Assert.AreEqual(firstUniqueId, model.GetUniqueId());

            shape = model.AddShape(new XElement("Test", new XAttribute("Id", firstUniqueId)));
            Assert.AreSame(shape, model.GetShapeById(firstUniqueId));

            // The second unique id should be auto_1:
            string secondUniqueId = model.GetUniqueId();
            Assert.AreEqual("auto_1", secondUniqueId);
            Assert.AreSame(null, model.GetShapeById(secondUniqueId));

            // Removing a shape should not cause the unique id to decrement.
            shape.Remove();
            Assert.AreEqual(secondUniqueId, model.GetUniqueId());

            DocumentDataModelInvariant(model);
            Assert.AreEqual(DataModel.ModelState.Ready, model.State);
        }

        /// <summary>
        ///A test for GetShapeById
        ///</summary>
        [TestMethod()]
        public void GetShapeByIdTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            target.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            XElement actual = target.GetShapeById("Id that isn't present");
            Assert.AreSame(null, actual);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            string knownId = "1";
            XElement shape = new XElement("testXElement", new XAttribute("Id", knownId));
            target.AddShape(shape);
            actual = target.GetShapeById(knownId);
            Assert.AreEqual(shape, actual);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
        }

        /// <summary>
        ///A test for EndOperationWithoutCreatingUndoState
        ///</summary>
        [TestMethod()]
        public void EndOperationWithoutCreatingUndoState()
        {
            DocumentDataModel target = new DocumentDataModel();
            target.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
            Assert.IsFalse(target.HasUndoData);

            target.BeginOperation("testoperation");
            target.EndOperationWithoutCreatingUndoState("testoperation");
            Assert.IsFalse(target.HasUndoData);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
        }

        /// <summary>
        ///A test for EndOperation
        ///</summary>
        [TestMethod()]
        public void EndOperationTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            target.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
            Assert.IsFalse(target.HasUndoData);

            //Test that undo state is created at end of operation.
            XElement expected = new XElement(target.DocumentRoot);
            target.BeginOperation("testoperation");
            target.AddShape(new XElement("testelement"));
            target.EndOperation("testoperation");
            Assert.IsTrue(target.HasUndoData);
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
            target.Undo(); //Undo should bring us back before the operation.
            AssertAreEqualStrings(expected, target.DocumentRoot);

        }

        /// <summary>
        ///A test for BeginOperation
        ///</summary>
        [TestMethod()]
        public void BeginOperationTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            target.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            //Test that one call makes the Model busy.
            target.BeginOperation("testoperation");
            Assert.AreEqual(DataModel.ModelState.Busy, target.State);
            DocumentDataModelInvariant(target);

            //Test that second call starts a nested operation.
            target.BeginOperation("nestedTestOperation");
            Assert.AreEqual(DataModel.ModelState.Busy, target.State);
            DocumentDataModelInvariant(target);
            target.EndOperation("nestedTestOperation");
            Assert.AreEqual(DataModel.ModelState.Busy, target.State);
            DocumentDataModelInvariant(target);
            target.EndOperation("testoperation");
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
            DocumentDataModelInvariant(target);
        }

        /// <summary>
        ///A test for AddShape
        ///</summary>
        [TestMethod()]
        public void AddShapeTest()
        {
            DocumentDataModel target = new DocumentDataModel();
            target.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            //Test if the reference returned is correct.
            XElement shape = new XElement("testTag", new XAttribute("Id", "myshape"));
            XElement addedShape = target.AddShape(shape);
            AssertAreEqualStrings(shape, addedShape);

            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);

            //Test if the id is replaced if it is taken.
            target.AddShape(shape);
            addedShape.Remove();
            Assert.AreSame(null, target.GetShapeById("myshape"));
            AssertAreEqualStrings(shape, target.GetShapeById("auto_0"));

            DocumentDataModelInvariant(target);
            Assert.AreEqual(DataModel.ModelState.Ready, target.State);
        }

        /// <summary>
        ///A test for State
        ///</summary>
        [TestMethod()]
        public void StateTest()
        {
            DocumentDataModel model = new DocumentDataModel();
            model.New(new Size(700, 1200), new Thickness(32, 32, 32, 32));
            DocumentDataModelInvariant(model);
            Assert.AreEqual(DataModel.ModelState.Ready, model.State);

            // Remove the PageHeight attribute, rendering the document invalid.
            model.DocumentRoot.SetAttributeValue("PageHeight", null);
            DocumentDataModelInvariant(model);
            Assert.AreEqual(DataModel.ModelState.Invalid, model.State);

            // Restore the PageHeight attribute, rendering the document valid.
            model.DocumentRoot.SetAttributeValue("PageHeight", 700);
            DocumentDataModelInvariant(model);
            Assert.AreEqual(DataModel.ModelState.Ready, model.State);
        }

        private delegate void ZeroArgDelegate();

        /// <summary>
        ///A test for PropertyChanged
        ///</summary>
        [TestMethod()]
        public void PropertyChangedTest()
        {
            DocumentDataModel model = new DocumentDataModel();

            string[] watchedProperties = new string[] { "HasUnsavedData", "HasUndoData", "HasRedoData", "DocumentRoot", "ObservableDocumentRoot" };
            Dictionary<string, object> propertyValues = new Dictionary<string,object>();
            HashSet<string> changedProperties = new HashSet<string>();

            PropertyChangedEventHandler propChangedHandler = delegate(object sender, PropertyChangedEventArgs e)
            {
                Assert.AreSame(model, sender);
                changedProperties.Add(e.PropertyName);
            };

            ZeroArgDelegate prepare = delegate()
            {
                // Note old property values for later comparison.
                foreach (string prop in watchedProperties)
                {
                    propertyValues[prop] = model.GetType().GetProperty(prop).GetValue(model, null);
                }
                changedProperties.Clear();
            };

            ZeroArgDelegate verify = delegate()
            {
                // Verify that properties are unchanged, or that we were notified that they changed.
                foreach (string prop in watchedProperties)
                {
                    if (changedProperties.Contains(prop)) continue; // We were notified of a change.

                    object oldValue = propertyValues[prop];
                    object newValue = model.GetType().GetProperty(prop).GetValue(model, null);
                    Assert.AreEqual(oldValue, newValue,
                            "Property {0} changed without notification, from '{1}' to '{2}'",
                            prop, oldValue, newValue);
                }
                DocumentDataModelInvariant(model);
            };

            model.PropertyChanged += propChangedHandler;


            prepare();
            model.New(new Size(100, 100), new Thickness(20));
            verify();
            // New should trigger updates on both DocumentRoot properties.
            Assert.IsTrue(changedProperties.Contains("DocumentRoot"));
            Assert.IsTrue(changedProperties.Contains("ObservableDocumentRoot"));

            prepare();
            model.AddShape(new XElement("Test"));
            verify();
            // A "deep" change of the document XML must not trigger updates on DocumentRoot, but only on ObservableDocumentRoot
            Assert.IsFalse(changedProperties.Contains("DocumentRoot"));
            Assert.IsTrue(changedProperties.Contains("ObservableDocumentRoot"));

            prepare();
            model.Undo();
            verify();
            // Undo should trigger updates on both DocumentRoot properties.
            Assert.IsTrue(changedProperties.Contains("DocumentRoot"));
            Assert.IsTrue(changedProperties.Contains("ObservableDocumentRoot"));

            prepare();
            model.Redo();
            verify();
            // Redo should trigger updates on both DocumentRoot properties.
            Assert.IsTrue(changedProperties.Contains("DocumentRoot"));
            Assert.IsTrue(changedProperties.Contains("ObservableDocumentRoot"));

            prepare();
            ((XElement) model.DocumentRoot.FirstNode).Name = "Test2";
            verify();
            // A "deep" change of the document XML must not trigger updates on DocumentRoot, but only on ObservableDocumentRoot
            Assert.IsFalse(changedProperties.Contains("DocumentRoot"));
            Assert.IsTrue(changedProperties.Contains("ObservableDocumentRoot"));

            // If we remove the handler, we should not get any notifications. :-)
            model.PropertyChanged -= propChangedHandler;
            prepare();
            model.Undo();

            Assert.AreEqual(0, changedProperties.Count); // No changes that we know of.
        }
    }
}
