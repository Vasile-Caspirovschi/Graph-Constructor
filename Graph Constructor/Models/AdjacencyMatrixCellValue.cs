using System.ComponentModel;

namespace Graph_Constructor.Models
{
    public class AdjacencyMatrixCellValue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int _value;
        public AdjacencyMatrixCellValue(int value)
        {
            _value = value;
        }

        public int Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged("Value"); }
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
