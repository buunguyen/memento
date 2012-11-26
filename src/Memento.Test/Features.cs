// ReSharper disable InconsistentNaming

namespace Memento.Test
{
    using System;
    using System.Linq;
    using Memento.Test.Stubs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Features
    {
        private Mementor m;

        [TestInitialize]
        public void Setup()
        {
            m = Session.New();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Session.End();
        }

        [TestMethod]
        public void Should_initialize_correctly()
        {
            UndoCount(0).RedoCount(0);
            Assert.IsTrue(m.IsTrackingEnabled);
        }

        [TestMethod]
        public void Should_undo_redo_property_change()
        {
            var c = new Circle();
            for (int i = 0; i < 10; i++) {
                c.Radius = i + 1;
                UndoCount(i + 1).RedoCount(0);
            }
            for (int i = 9; i >= 0; i--) {
                m.Undo();
                Assert.AreEqual(i, c.Radius);
                UndoCount(i).RedoCount(9 - i + 1);
            }
            for (int i = 0; i < 10; i++) {
                m.Redo();
                Assert.AreEqual(i + 1, c.Radius);
                UndoCount(i + 1).RedoCount(9 - i);
            }
        }

        [TestMethod]
        public void Should_allow_provide_property_value()
        {
            var c = new Circle();
            UndoCount(0);
            m.PropertyChange(c, () => c.Radius, 10);
            m.Undo();
            Assert.AreEqual(10, c.Radius);
        }

        [TestMethod]
        public void Should_undo_redo_complex_property_change()
        {
            var c = new Circle();
            for (int i = 0; i < 10; i++) {
                c.Center = new Point(i + 1, i + 1);
                UndoCount(i + 1).RedoCount(0);
            }
            for (int i = 9; i >= 0; i--) {
                m.Undo();
                Assert.AreEqual(new Point(i, i), c.Center);
                UndoCount(i).RedoCount(9 - i + 1);
            }
            for (int i = 0; i < 10; i++) {
                m.Redo();
                Assert.AreEqual(new Point(i + 1, i + 1), c.Center);
                UndoCount(i + 1).RedoCount(9 - i);
            }
        }

        [TestMethod]
        public void Should_undo_multiple_properties_change()
        {
            var c = new Circle {Radius = 10, Center = new Point(10, 10)};
            UndoCount(2);

            m.Undo();
            Assert.AreEqual(new Point(0, 0), c.Center);
            UndoCount(1);

            m.Undo();
            Assert.AreEqual(0, c.Radius);
            UndoCount(0);
        }

        [TestMethod]
        public void Should_reset_to_initial_states()
        {
            new Circle {Radius = 10, Center = new Point(10, 10)};
            UndoCount(2);

            m.Reset();
            UndoCount(0).RedoCount(0);
        }

        [TestMethod]
        public void Should_clear_redo_after_a_forward_change()
        {
            var c = new Circle {Radius = 10};
            UndoCount(1).RedoCount(0);

            m.Undo();
            UndoCount(0).RedoCount(1);

            c.Radius++;
            UndoCount(1).RedoCount(0);
        }

        [TestMethod]
        public void Should_be_able_to_piggy_back_undo_redo()
        {
            var c = new Circle {Radius = 10};
            UndoCount(1).RedoCount(0);

            m.Undo();
            Assert.AreEqual(0, c.Radius);
            UndoCount(0).RedoCount(1);

            m.Redo();
            Assert.AreEqual(10, c.Radius);
            UndoCount(1).RedoCount(0);

            m.Undo();
            Assert.AreEqual(0, c.Radius);
            UndoCount(0).RedoCount(1);

            m.Redo();
            Assert.AreEqual(10, c.Radius);
            UndoCount(1).RedoCount(0);
        }

        [TestMethod]
        public void Should_undo_redo_whole_batch()
        {
            var circles = new Circle[10];
            for (int i = 0; i < circles.Length; i++) {
                circles[i] = new Circle();
            }

            m.Batch(() => {
                foreach (Circle circle in circles) {
                    circle.Radius = 5;
                    circle.Center = new Point(5, 5);
                }
            });
            UndoCount(1);

            m.Undo();
            foreach (Circle circle in circles) {
                Assert.AreEqual(0, circle.Radius);
                Assert.AreEqual(new Point(0, 0), circle.Center);
            }
            RedoCount(1);

            m.Redo();
            foreach (Circle circle in circles) {
                Assert.AreEqual(5, circle.Radius);
                Assert.AreEqual(new Point(5, 5), circle.Center);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public void Should_throw_if_nesting_batches()
        {
            m.Batch(() => m.Batch(() => new Circle() {Radius = 5}));
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public void Should_throw_if_nesting_batches_via_explicit_calls()
        {
            m.BeginBatch();
            m.BeginBatch();
            m.EndBatch();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Should_throw_if_end_batch_without_starting_one()
        {
            m.EndBatch();
        }

        [TestMethod]
        public void Should_not_throw_if_end_batch_after_starting_one()
        {
            for (var i = 0; i < 10; i++) {
                m.BeginBatch();
                m.EndBatch();
            }
        }

        [TestMethod]
        public void Should_track_based_on_enabling_setting()
        {
            m.IsTrackingEnabled = false;
            new Circle {Radius = 5};
            UndoCount(0);

            m.IsTrackingEnabled = true;
            new Circle {Radius = 5};
            UndoCount(1);
        }

        [TestMethod]
        public void Should_not_track_during_a_none_tracking_execution()
        {
            Assert.IsTrue(m.IsTrackingEnabled);
            m.ExecuteNoTrack(() => {
                Assert.IsFalse(m.IsTrackingEnabled);
                new Circle {Radius = 5, Center = new Point(5, 5)};
            });
            Assert.IsTrue(m.IsTrackingEnabled);
            UndoCount(0);
        }

        [TestMethod]
        public void Should_allow_nested_disabling_tracking()
        {
            Assert.IsTrue(m.IsTrackingEnabled);
            m.ExecuteNoTrack(() => {
                new Circle {Radius = 5, Center = new Point(5, 5)};
                m.ExecuteNoTrack(() => { new Circle {Radius = 5, Center = new Point(5, 5)}; });
            });
            Assert.IsTrue(m.IsTrackingEnabled);
            UndoCount(0);
        }

        [TestMethod]
        public void Should_restore_to_previous_tracking_states_in_a_recursive_manner()
        {
            m.IsTrackingEnabled = false;
            m.ExecuteNoTrack(() => {
                m.IsTrackingEnabled = true;
                m.ExecuteNoTrack(() => { });
                Assert.IsTrue(m.IsTrackingEnabled);
            });
            Assert.IsFalse(m.IsTrackingEnabled);
        }

        [TestMethod]
        public void Should_allow_temporary_enabling_during_no_track_context()
        {
            m.ExecuteNoTrack(() => {
                var c = new Circle {Radius = 5};
                m.IsTrackingEnabled = true;
                c.Radius++;
                m.IsTrackingEnabled = false;
                c.Radius++;
            });
            UndoCount(1);
        }

        [TestMethod]
        public void Should_undo_redo_collection_addition()
        {
            var screen = new Screen();
            var circle = new Circle();
            screen.Add(circle);
            UndoCount(1);

            m.Undo();
            Assert.AreEqual(0, screen.Shapes.Count);

            m.Redo();
            Assert.AreSame(circle, screen.Shapes[0]);
        }

        [TestMethod]
        public void Should_undo_redo_collection_removal()
        {
            var screen = new Screen();
            var circle = new Circle();
            screen.Add(circle);
            m.Reset();

            screen.Remove(circle);
            UndoCount(1);

            m.Undo();
            Assert.AreSame(circle, screen.Shapes[0]);

            m.Redo();
            Assert.AreEqual(0, screen.Shapes.Count);
        }

        [TestMethod]
        public void Should_undo_redo_collection_position_change()
        {
            var screen = new Screen();
            Circle circle1, circle2;
            screen.Add(circle1 = new Circle());
            screen.Add(circle2 = new Circle());
            m.Reset();

            screen.MoveToFront(1);
            Assert.AreSame(circle2, screen.Shapes[0]);
            Assert.AreSame(circle1, screen.Shapes[1]);

            m.Undo();
            Assert.AreSame(circle1, screen.Shapes[0]);
            Assert.AreSame(circle2, screen.Shapes[1]);

            m.Redo();
            Assert.AreSame(circle2, screen.Shapes[0]);
            Assert.AreSame(circle1, screen.Shapes[1]);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void Should_throw_when_removing_non_existent_element()
        {
            var screen = new Screen();
            screen.Remove(new Circle());
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void Should_throw_when_changing_position_of_non_existent_element()
        {
            var screen = new Screen();
            screen.Add(new Circle());
            m.ElementIndexChange(screen.Shapes, new Circle());
        }

        [TestMethod]
        public void Should_undo_redo_collection_changes_in_batch()
        {
            var screen = new Screen();
            m.Batch(() => {
                var circle = new Circle();
                screen.Add(new Circle {Radius = 10});
                screen.Add(circle);
                screen.MoveToFront(1);
                screen.Remove(circle);
            });
            Assert.AreEqual(1, screen.Shapes.Count);
            UndoCount(1);

            m.Undo();
            Assert.AreEqual(0, screen.Shapes.Count);
        }

        [TestMethod]
        public void Should_undo_redo_collection_changes_in_explicit_batch()
        {
            var screen = new Screen();
            m.BeginBatch();
            try {
                var circle = new Circle();
                screen.Add(new Circle {Radius = 10});
                screen.Add(circle);
                screen.MoveToFront(1);
                screen.Remove(circle);
            } finally {
                m.EndBatch();
            }
            Assert.AreEqual(1, screen.Shapes.Count);
            UndoCount(1);

            m.Undo();
            Assert.AreEqual(0, screen.Shapes.Count);
        }

        [TestMethod]
        public void Should_fire_events()
        {
            int count = 0;
            m.Changed += (_, args) => count++;

            var circle = new Circle {Radius = 5};
            Assert.AreEqual(1, count);

            circle.Center = new Point(5, 5);
            Assert.AreEqual(2, count);

            m.Batch(() => new Circle {Radius = 5, Center = new Point(5, 5)});
            Assert.AreEqual(3, count);

            m.Undo();
            Assert.AreEqual(4, count);

            m.Redo();
            Assert.AreEqual(5, count);

            m.IsTrackingEnabled = false;
            new Circle {Radius = 5};
            Assert.AreEqual(5, count);
            m.IsTrackingEnabled = true;

            m.ExecuteNoTrack(() => new Circle {Radius = 5, Center = new Point()});
            Assert.AreEqual(5, count);

            m.Reset();
            Assert.AreEqual(6, count);
        }

        [TestMethod]
        public void Should_fire_property_change_event()
        {
            new Circle {Radius = 10};
            m.Changed += (_, args) => {
                Assert.AreEqual(typeof (PropertyChangeEvent), args.Event.GetType());
                Assert.AreEqual(0, ((PropertyChangeEvent) args.Event).PropertyValue);
            };
            m.Undo();
        }

        [TestMethod]
        public void Should_fire_collection_addition_event()
        {
            var screen = new Screen();
            var circle = new Circle();

            int count = 0;
            m.Changed += (_, args) => {
                Assert.AreEqual(typeof(ElementAdditionEvent<Circle>), args.Event.GetType());
                Assert.AreSame(screen.Shapes, ((ElementAdditionEvent<Circle>)args.Event).Collection);
                Assert.AreSame(circle, ((ElementAdditionEvent<Circle>)args.Event).Element);
                count++;
            };
            screen.Add(circle);
            m.Undo();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Should_fire_collection_removal_event()
        {
            var screen = new Screen();
            var circle = new Circle();
            screen.Add(circle);

            int count = 0;
            m.Changed += (_, args) => {
                Assert.AreEqual(typeof (ElementRemovalEvent<Circle>), args.Event.GetType());
                Assert.AreSame(screen.Shapes, ((ElementRemovalEvent<Circle>)args.Event).Collection);
                Assert.AreSame(circle, ((ElementRemovalEvent<Circle>)args.Event).Element);
                Assert.AreEqual(0, ((ElementRemovalEvent<Circle>)args.Event).Index);
                count++;
            };
            screen.Remove(circle);
            m.Undo();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Should_fire_collection_element_position_change_event()
        {
            var screen = new Screen();
            var circle = new Circle();
            screen.Add(new Circle());
            screen.Add(circle);

            int count = 0;
            m.Changed += (_, args) => {
                Assert.AreEqual(typeof(ElementIndexChangeEvent<Circle>), args.Event.GetType());
                Assert.AreSame(screen.Shapes, ((ElementIndexChangeEvent<Circle>)args.Event).Collection);
                Assert.AreSame(circle, ((ElementIndexChangeEvent<Circle>)args.Event).Element);
                Assert.AreEqual(1, ((ElementIndexChangeEvent<Circle>)args.Event).Index);
                count++;
            };
            screen.MoveToFront(1);
            m.Undo();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Should_fire_batch_event()
        {
            int count = 0;
            m.Changed += (_, args) => {
                Assert.AreEqual(typeof (BatchEvent), args.Event.GetType());
                Assert.AreEqual(2, ((BatchEvent) args.Event).Count);
                var events = ((BatchEvent) args.Event).ToArray();
                Assert.AreEqual(typeof (PropertyChangeEvent), events[0].GetType());
                Assert.AreEqual(typeof (PropertyChangeEvent), events[1].GetType());
                count++;
            };
            m.Batch(() => new Circle {Center = new Point(5, 5), Radius = 5});
            m.Undo();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Should_handle_custom_event()
        {
            var reverseEvent = new CustomEvent(null);
            var @event = new CustomEvent(reverseEvent);
            m.MarkEvent(@event);
            UndoCount(1);
            
            m.Undo();
            Assert.IsTrue(@event.IsRolledback);

            m.Changed += (_, args) => Assert.AreSame(reverseEvent, args.Event);
            m.Redo();
        }

        [TestMethod]
        public void Should_throw_if_invalid_rollback_return_type()
        {
            m.Batch(() => {
                new Circle() {Radius = 10, Center = new Point(10, 10)};
            });
            BatchEvent batchEvent = null;
            MementorChanged changed = (_, args) => {
                batchEvent = (BatchEvent) args.Event;
            };
            m.Changed += changed;
            m.Undo();
            Assert.IsNotNull(batchEvent);

            m.Changed -= changed;
            var customEvent = new InvalidCustomEvent(batchEvent);
            m.MarkEvent(customEvent);

            try
            {
                m.Undo();
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException) {
            }

            m.Reset();
            m.Batch(() => {
                m.MarkEvent(customEvent);
                m.MarkEvent(customEvent);
            });
            try
            {
                m.Undo();
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
            }
        }

        #region Helper

        private Features UndoCount(int c)
        {
            Assert.AreEqual(c, m.UndoCount);
            Assert.AreEqual(c > 0, m.CanUndo);
            return this;
        }

        private Features RedoCount(int c)
        {
            Assert.AreEqual(c, m.RedoCount);
            Assert.AreEqual(c > 0, m.CanRedo);
            return this;
        }

        #endregion
    }
}

// ReSharper restore InconsistentNaming