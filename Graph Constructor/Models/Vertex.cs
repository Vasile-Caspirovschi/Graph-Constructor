
using System.ComponentModel;
using System.Windows;

namespace Graph_Constructor.Models
{
    public class Vertex : INotifyPropertyChanged
    {
        private const int DIAMETER = 30;
        private int _id;
        private Point _location;
        public Vertex(int id, Point location)
        {
            _id = id;
            _location.X = location.X - DIAMETER / 2;
            _location.Y = location.Y - DIAMETER / 2;
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged("Id"); }
        }
        public Point Location { get { return _location; } }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
