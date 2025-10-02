namespace ParkingLot.Entity
{
    public class Lot
    {
        public string Id { get; private set; }
        public int Floors { get; private set; }
        public int FloorCapacity { get; private set; }

        public Lot(string id, int floors, int floorCapacity)
        {
            Id = id;
            Floors = floors;
            FloorCapacity = floorCapacity;
        }
    }
}