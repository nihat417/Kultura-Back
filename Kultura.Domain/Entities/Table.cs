using Kultura.Domain.Enums;

namespace Kultura.Domain.Entities
{
    public class Table : BaseEntity
    {
        private int _capacity;
        public int Capacity
        {
            get => _capacity;
            set
            {
                if (value < 1 || value > 5)
                    throw new ArgumentException("Table capacity must be between 1 and 5.");

                _capacity = value;

                switch (_capacity)
                {
                    case 2:
                        ShapeType = TableShapeType.Square;
                        break;
                    case 3:
                        ShapeType = TableShapeType.Triangle;
                        break;
                    case 5:
                        ShapeType = TableShapeType.Rectangle;
                        break;
                    default:
                        ShapeType = TableShapeType.Rectangle; 
                        break;
                }
            }
        }

        private int _floorNumber;
        public int FloorNumber
        {
            get => _floorNumber;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Floor number must be greater than or equal to 1.");
                _floorNumber = value;
            }
        }

        public virtual Floor Floor { get; set; } = default!;

        public virtual List<Reservation> Reservations { get; set; } = new();

        public int X { get; set; }
        public int Y { get; set; }
        public double Radius { get; set; }

        public TableShapeType ShapeType { get; set; } = TableShapeType.Rectangle;
    }
}
