namespace Plain
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    internal class Class : ObservableCollection<Student>
    {
    }

    internal class Student : INotifyPropertyChanging
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value) {
                    OnPropertyChanging("Name");
                    _name = value;
                }
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        private void OnPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }
    }
}
