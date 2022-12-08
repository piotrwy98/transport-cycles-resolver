using System.Windows;

namespace TransportCyclesResolver.Models
{
    public class Field : NotifyPropertyChanged
    {
        public int X { get; set; }

        public int Y { get; set; }

        public double? Value { get; set; }

        public int? _cycleIndex;
        public int? CycleIndex
        {
            get
            {
                return _cycleIndex;
            }
            set
            {
                _cycleIndex = value;
                OnPropertyChanged();
            }
        }

        public bool IsEmpty => !Value.HasValue;

        private bool _isHovered;
        public bool IsHovered
        {
            get
            {
                return _isHovered;
            }
            set
            {
                _isHovered = value;
                OnPropertyChanged();
            }
        }

        private Visibility _downLineVisibility = Visibility.Collapsed;
        public Visibility DownLineVisibility
        {
            get
            {
                return _downLineVisibility;
            }
            set
            {
                _downLineVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _upLineGoDownVisibility = Visibility.Collapsed;
        public Visibility UpLineGoDownVisibility
        {
            get
            {
                return _upLineGoDownVisibility;
            }
            set
            {
                _upLineGoDownVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _upLineGoUpVisibility = Visibility.Collapsed;
        public Visibility UpLineGoUpVisibility
        {
            get
            {
                return _upLineGoUpVisibility;
            }
            set
            {
                _upLineGoUpVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _rightLineGoRightVisibility = Visibility.Collapsed;
        public Visibility RightLineGoRightVisibility
        {
            get
            {
                return _rightLineGoRightVisibility;
            }
            set
            {
                _rightLineGoRightVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _rightLineGoLeftVisibility = Visibility.Collapsed;
        public Visibility RightLineGoLeftVisibility
        {
            get
            {
                return _rightLineGoLeftVisibility;
            }
            set
            {
                _rightLineGoLeftVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _leftLineVisibility = Visibility.Collapsed;
        public Visibility LeftLineVisibility
        {
            get
            {
                return _leftLineVisibility;
            }
            set
            {
                _leftLineVisibility = value;
                OnPropertyChanged();
            }
        }
    }
}
