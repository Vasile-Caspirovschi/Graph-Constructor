
using Graph_Constructor.Interfaces;
using System.ComponentModel;
using System.Windows;

namespace Graph_Constructor.Models
{
    public class Vertex : INotifyPropertyChanged, IMarkable
    {
        private const int DIAMETER = 30;
        private int _id;
        private Point _location;
        private Point _leftTop;
        public Vertex(int id, Point location, Point leftTop)
        {
            _id = id;
            _location.X = location.X - DIAMETER / 2;
            _location.Y = location.Y - DIAMETER / 2;
            _leftTop = leftTop;
        }
        public Vertex(int id, Point location)
        {
            _id = id;
            _location.X = location.X - DIAMETER / 2;
            _location.Y = location.Y - DIAMETER / 2;
            _leftTop = new Point(0,0);
        }

        public Vertex(int id)
        {
            _id = id;
        }

        public Vertex TraverseParent { get; set; }

        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged("Id"); }
        }
        public Point Location { get { return _location; } set { _location = value; } }

        public Point LeftTop { get => _leftTop; set => _leftTop = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsVertex()
        {
            return true;
        }

        public bool IsWeightedEdge()
        {
            return false;
        }

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
