using ParkingLot.Entity.Enum;

namespace ParkingLot.Entity
{
    public class Slot
    {
        public string Id { get; private set; }
        public string LotId { get; private set; }
        public int Floor { get; private set; }
        public int SlotNumber { get; private set; }
        public int Rank { get; private set; }
        public VehicleType VehicleType { get; private set; }
        public SlotStatus Status { get; private set; }

        public Slot(VehicleType vehicleType, string lotId, int floor, int slotNumber, int rank)
        {
            Id = $"{lotId}_{floor}_{slotNumber}";
            VehicleType = vehicleType;
            LotId = lotId;
            Floor = floor;
            SlotNumber = slotNumber;
            Status = SlotStatus.Available;
            Rank = rank;
        }

        public void ParkVehicle()
        {
            Status = SlotStatus.Occupied;
        }

        public void RemoveVehicle()
        {
            Status = SlotStatus.Available;
        }
    }
}