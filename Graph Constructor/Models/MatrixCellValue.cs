using Graph_Constructor.Enums;
using System.ComponentModel;

namespace Graph_Constructor.Models
{
    public class MatrixCellValue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int _value;
        private GraphType _graphType;

        public GraphType GraphType { get => _graphType; set => _graphType = value; }
        public string CellId { get; set; }
        public MatrixCellValue(int value)
        {
            _value = value;
        }
        public MatrixCellValue(int value, string cellId, GraphType graphType)
        {
            _value = value;
            CellId = cellId;
            GraphType = graphType;
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
