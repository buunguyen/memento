namespace Memento.Test.Stubs
{
    internal class Circle
    {
        private Point _center;

        public Point Center
        {
            get { return _center; }
            set
            {
                Session.Mementor.PropertyChange(this, () => Center);
                _center = value;
            }
        }

        private int _radius;

        public int Radius
        {
            get { return _radius; }
            set
            {
                Session.Mementor.PropertyChange(this, () => Radius);
                _radius = value;
            }
        }
    }
}