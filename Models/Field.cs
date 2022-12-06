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
    }
}
