namespace Plain
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using Memento;

    /// <summary>
    /// While Mementor can be used with POCOs (as demonstrated in the unit test suite), 
    /// this sample program demonstrates how to use Mementor with objects implementing 
    /// <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanging"/>
    /// just via event handling instead of mixing Memento calls into model classes.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// <see cref="NotifyCollectionChangedAction.Remove"/> doesn't maintain old index,
        /// so use this cache in order to retrieve index whenever marking a removal event.
        /// </summary>
        private static ObservableCollection<Student> Cache;
        private static readonly Mementor M = new Mementor();

        private static void Main()
        {
            var cls = new Class();
            Cache = new ObservableCollection<Student>(cls);
            cls.CollectionChanged += ClassChanged;

            cls.Add(new Student());
            cls.Add(new Student());
            cls[0].Name = "Peter";
            cls[1].Name = "John";
            cls.Move(0, 1);
            cls.RemoveAt(1);

            Dump(cls);

            M.Changed += (_, args) => Console.WriteLine("Undo event: " + args.Event.GetType());
            while (M.CanUndo) {
                M.Undo();
                Dump(cls);
            }
        }

        private static void Dump(Class cls)
        {
            Console.WriteLine("Class information");
            Console.WriteLine("Count: " + cls.Count);
            for (int i = 0; i < cls.Count; i++) {
                Console.WriteLine("Student[{0}]: {1}", i+1, cls[i].Name);
            }
            Console.WriteLine();
        }

        private static void ClassChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action) {
                case NotifyCollectionChangedAction.Move:
                    M.ElementIndexChange((Class) sender, (Student) args.OldItems[0], args.OldStartingIndex);
                    Cache = new ObservableCollection<Student>((Class)sender);
                    break;
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Remove:
                    if (args.OldItems != null) {
                        foreach (var student in args.OldItems.Cast<Student>()) {
                            M.ElementRemove((Class)sender, student, Cache.IndexOf(student));
                            student.PropertyChanging -= StudentChanging;
                        }
                    }

                    if (args.NewItems != null) {
                        foreach (var student in args.NewItems.Cast<Student>()) {
                            M.ElementAdd((Class) sender, student);
                            student.PropertyChanging += StudentChanging;
                        }
                    }

                    Cache = new ObservableCollection<Student>((Class)sender);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private static void StudentChanging(object sender, PropertyChangingEventArgs args)
        {
            M.PropertyChange(sender, args.PropertyName);
        }
    }
}