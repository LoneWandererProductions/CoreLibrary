/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Tests
 * FILE:        HistoryTests.cs
 * PURPOSE:     Commands and the undo/redo stack.
 */

using Imaging.Objects.Commands;
using Imaging.Objects.Documents;
using Imaging.Objects.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imaging.Objects.Tests
{
    [TestClass]
    public sealed class HistoryTests
    {
        private static Document NewDocument(out DocumentHistory history, int limit = 50)
        {
            var document = new Document(8, 8);
            history = new DocumentHistory(document, limit);
            return document;
        }

        [TestMethod]
        public void AddLayer_UndoRedo()
        {
            using var document = NewDocument(out var history);
            var layer = new ShapeLayer("a");

            history.Execute(new AddLayerCommand(layer));
            Assert.AreEqual(1, document.Layers.Count);

            history.Undo();
            Assert.AreEqual(0, document.Layers.Count);
            Assert.IsTrue(history.CanRedo);

            history.Redo();
            Assert.AreSame(layer, document.Layers[0]);
        }

        [TestMethod]
        public void RemoveLayer_UndoRestoresLayerAtItsOldIndex()
        {
            using var document = NewDocument(out var history);
            var a = new ShapeLayer("a");
            var b = new ShapeLayer("b");
            var c = new ShapeLayer("c");
            history.Execute(new AddLayerCommand(a));
            history.Execute(new AddLayerCommand(b));
            history.Execute(new AddLayerCommand(c));

            history.Execute(new RemoveLayerCommand(b.Id));
            Assert.AreEqual(2, document.Layers.Count);

            history.Undo();
            Assert.AreSame(b, document.Layers[1]);

            history.Redo();
            Assert.AreEqual(2, document.Layers.Count);
            Assert.AreSame(c, document.Layers[1]);
        }

        [TestMethod]
        public void MoveLayer_UndoRedo()
        {
            using var document = NewDocument(out var history);
            var a = new ShapeLayer("a");
            var b = new ShapeLayer("b");
            history.Execute(new AddLayerCommand(a));
            history.Execute(new AddLayerCommand(b));

            history.Execute(new MoveLayerCommand(a.Id, 1));
            Assert.AreSame(a, document.Layers[1]);

            history.Undo();
            Assert.AreSame(a, document.Layers[0]);

            history.Redo();
            Assert.AreSame(a, document.Layers[1]);
        }

        [TestMethod]
        public void SetLayerProperties_UndoRedo()
        {
            using var document = NewDocument(out var history);
            var layer = new ShapeLayer("a");
            history.Execute(new AddLayerCommand(layer));

            history.Execute(new SetLayerPropertiesCommand(layer.Id,
                new LayerProperties("renamed", false, 0.25, BlendMode.Normal)));
            Assert.AreEqual("renamed", layer.Name);
            Assert.IsFalse(layer.Visible);
            Assert.AreEqual(0.25, layer.Opacity, 0.0001);

            history.Undo();
            Assert.AreEqual("a", layer.Name);
            Assert.IsTrue(layer.Visible);
            Assert.AreEqual(1d, layer.Opacity, 0.0001);

            history.Redo();
            Assert.AreEqual("renamed", layer.Name);
        }

        [TestMethod]
        public void ShapeCommands_UndoRedo()
        {
            using var document = NewDocument(out var history);
            var layer = new ShapeLayer("ink");
            history.Execute(new AddLayerCommand(layer));

            var rect = new RectShape(1, 1, 3, 3);
            history.Execute(new AddShapeCommand(layer.Id, rect));
            Assert.AreEqual(1, layer.Shapes.Count);

            history.Execute(new ReplaceShapeCommand(layer.Id, rect.Translate(2, 0)));
            Assert.AreEqual(3d, ((RectShape)layer.Shapes[0]).X, 0.0001);

            history.Execute(new RemoveShapeCommand(layer.Id, rect.Id));
            Assert.AreEqual(0, layer.Shapes.Count);

            history.Undo();
            Assert.AreEqual(3d, ((RectShape)layer.Shapes[0]).X, 0.0001);

            history.Undo();
            Assert.AreEqual(1d, ((RectShape)layer.Shapes[0]).X, 0.0001);

            history.Undo();
            Assert.AreEqual(0, layer.Shapes.Count);
        }

        [TestMethod]
        public void ShapeCommands_ReportDirtyRegion()
        {
            using var document = NewDocument(out var history);
            var layer = new ShapeLayer("ink");
            history.Execute(new AddLayerCommand(layer));

            var regions = new List<PixelRect>();
            document.Changed += (_, e) =>
            {
                if (e.Kind == DocumentChangeKind.ShapesChanged) regions.Add(e.Region);
            };

            var rect = new RectShape(0, 0, 2, 2);
            history.Execute(new AddShapeCommand(layer.Id, rect));
            history.Execute(new ReplaceShapeCommand(layer.Id, rect.Translate(4, 4)));

            // The replace must cover the old and the new position so both get redrawn.
            var replace = regions[1];
            Assert.IsTrue(replace.X <= rect.GetBounds().X && replace.Right >= rect.Translate(4, 4).GetBounds().Right);
        }

        [TestMethod]
        public void NewCommand_ClearsRedoBranch()
        {
            using var document = NewDocument(out var history);
            history.Execute(new AddLayerCommand(new ShapeLayer("a")));
            history.Execute(new AddLayerCommand(new ShapeLayer("b")));
            history.Undo();
            Assert.IsTrue(history.CanRedo);

            history.Execute(new AddLayerCommand(new ShapeLayer("c")));

            Assert.IsFalse(history.CanRedo);
            Assert.AreEqual("c", document.Layers[1].Name);
        }

        [TestMethod]
        public void Limit_TrimsOldestSteps()
        {
            using var document = NewDocument(out var history, limit: 2);
            history.Execute(new AddLayerCommand(new ShapeLayer("1")));
            history.Execute(new AddLayerCommand(new ShapeLayer("2")));
            history.Execute(new AddLayerCommand(new ShapeLayer("3")));

            history.Undo();
            history.Undo();

            Assert.IsFalse(history.CanUndo);
            Assert.AreEqual(1, document.Layers.Count);
            Assert.AreEqual("1", document.Layers[0].Name);
        }

        [TestMethod]
        public void FailedExecute_RecordsNothing()
        {
            using var document = NewDocument(out var history);

            TestSupport.Throws<InvalidOperationException>(() => history.Execute(new TestSupport.FailingCommand()));

            Assert.IsFalse(history.CanUndo);
            Assert.IsFalse(history.IsDirty);
        }

        [TestMethod]
        public void Descriptions_FollowTheStacks()
        {
            using var document = NewDocument(out var history);
            Assert.IsNull(history.UndoDescription);

            history.Execute(new AddLayerCommand(new ShapeLayer("a")));
            Assert.AreEqual("Add layer", history.UndoDescription);

            history.Undo();
            Assert.IsNull(history.UndoDescription);
            Assert.AreEqual("Add layer", history.RedoDescription);
        }

        [TestMethod]
        public void Changed_IsRaisedWhenStateChanges()
        {
            using var document = NewDocument(out var history);
            var count = 0;
            history.Changed += (_, _) => count++;

            history.Execute(new AddLayerCommand(new ShapeLayer("a")));
            history.Undo();
            history.Redo();
            history.MarkClean();

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void IsDirty_TracksTheSavedState()
        {
            using var document = NewDocument(out var history);
            Assert.IsFalse(history.IsDirty);

            history.Execute(new AddLayerCommand(new ShapeLayer("a")));
            Assert.IsTrue(history.IsDirty);

            history.MarkClean();
            Assert.IsFalse(history.IsDirty);

            history.Execute(new AddLayerCommand(new ShapeLayer("b")));
            Assert.IsTrue(history.IsDirty);

            history.Undo();
            Assert.IsFalse(history.IsDirty);

            history.Undo();
            Assert.IsTrue(history.IsDirty);

            history.Redo();
            Assert.IsFalse(history.IsDirty);
        }

        [TestMethod]
        public void IsDirty_StaysDirtyWhenSavedStateWasDiscarded()
        {
            using var document = NewDocument(out var history);
            history.Execute(new AddLayerCommand(new ShapeLayer("a")));
            history.Execute(new AddLayerCommand(new ShapeLayer("b")));
            history.MarkClean();

            history.Undo();
            history.Undo();
            history.Execute(new AddLayerCommand(new ShapeLayer("c")));

            // The saved state (a, b) is gone for good.
            Assert.IsTrue(history.IsDirty);
            history.Undo();
            Assert.IsTrue(history.IsDirty);
        }

        [TestMethod]
        public void IsDirty_StaysDirtyWhenSavedStateWasTrimmed()
        {
            using var document = NewDocument(out var history, limit: 2);
            history.MarkClean();
            history.Execute(new AddLayerCommand(new ShapeLayer("1")));
            history.Execute(new AddLayerCommand(new ShapeLayer("2")));
            history.Execute(new AddLayerCommand(new ShapeLayer("3")));

            history.Undo();
            history.Undo();

            Assert.IsTrue(history.IsDirty);
        }

        [TestMethod]
        public void Clear_KeepsDirtyFlag()
        {
            using var document = NewDocument(out var history);
            history.Execute(new AddLayerCommand(new ShapeLayer("a")));

            history.Clear();
            Assert.IsFalse(history.CanUndo);
            Assert.IsTrue(history.IsDirty);

            history.MarkClean();
            history.Clear();
            Assert.IsFalse(history.IsDirty);
        }

        [TestMethod]
        public void DiscardedRedoBranch_DisposesTheUndoneLayer()
        {
            using var document = NewDocument(out var history);
            var undone = new RasterLayer(8, 8, "undone");
            history.Execute(new AddLayerCommand(undone));
            history.Undo();
            Assert.IsFalse(undone.IsDisposed, "Still reachable through redo.");

            history.Execute(new AddLayerCommand(new ShapeLayer("other")));

            Assert.IsTrue(undone.IsDisposed);
        }

        [TestMethod]
        public void TrimmedRemove_DisposesTheRemovedLayer()
        {
            using var document = NewDocument(out var history, limit: 2);
            var victim = new RasterLayer(8, 8, "victim");
            var keep = new ShapeLayer("keep");
            history.Execute(new AddLayerCommand(victim));
            history.Execute(new AddLayerCommand(keep));
            history.Execute(new RemoveLayerCommand(victim.Id));
            Assert.IsFalse(victim.IsDisposed, "The remove is still the newest step.");

            history.Execute(new SetLayerPropertiesCommand(keep.Id,
                new LayerProperties("x", true, 1, BlendMode.Normal)));
            history.Execute(new SetLayerPropertiesCommand(keep.Id,
                new LayerProperties("y", true, 1, BlendMode.Normal)));

            Assert.IsTrue(victim.IsDisposed);
        }

        [TestMethod]
        public void AppliedAdd_IsNotDisposedByHistory()
        {
            var document = NewDocument(out var history, limit: 1);
            var layer = new RasterLayer(8, 8, "kept");
            history.Execute(new AddLayerCommand(layer));
            history.Execute(new AddLayerCommand(new ShapeLayer("second")));

            // The first step was trimmed but its layer belongs to the document now.
            Assert.IsFalse(layer.IsDisposed);

            history.Dispose();
            Assert.IsFalse(layer.IsDisposed);
            document.Dispose();
            Assert.IsTrue(layer.IsDisposed);
        }
    }
}
