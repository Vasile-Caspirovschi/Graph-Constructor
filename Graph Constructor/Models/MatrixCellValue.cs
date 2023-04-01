using System.ComponentModel;

namespace Graph_Constructor.Models
{
    public class MatrixCellValue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int _value;
        public string CellId { get; set; }
        public MatrixCellValue(int value)
        {
            _value = value;
        }
        public MatrixCellValue(int value, string cellId)
        {
            _value = value;
            CellId = cellId;
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
