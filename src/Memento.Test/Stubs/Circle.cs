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
                if (_center != value) {
                    Session.Mementor.PropertyChange(this, () => Center);
                    _center = value;
                }
            }
        }

        private int _radius;

        public int Radius
        {
            get { return _radius; }
            set
            {
                if (_radius != value) {
                    Session.Mementor.PropertyChange(this, () => Radius);
                    _radius = value;
                }
            }
        }
    }
}