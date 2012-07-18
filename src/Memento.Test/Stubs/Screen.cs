namespace Memento.Test.Stubs
{
    using System.Collections.Generic;

    internal class Screen
    {
        public Screen()
        {
            Shapes = new List<Circle>();
        }

        public IList<Circle> Shapes { get; set; }

        public void Add(Circle circle)
        {
            Session.Mementor.ElementAdd(Shapes, circle);
            Shapes.Add(circle);
        }

        public void Remove(Circle circle)
        {
            Session.Mementor.ElementRemove(Shapes, circle);
            Shapes.Remove(circle);
        }

        public void MoveToFront(int index)
        {
            Circle circle = Shapes[index];
            Session.Mementor.ElementIndexChange(Shapes, circle);
            Shapes.Remove(circle);
            Shapes.Insert(0, circle);
        }
    }
}