using ParkingLot.Entity.Enum;

namespace ParkingLot.BusinessLayer
{
    public sealed class FirstAvailableSlotAssignmentStrategy : ISlotAssignmentStrategy
    {
        public VehicleType ClassifyBySlotNumber(int slotNumber)
        {
            return slotNumber switch
            {
                1 => VehicleType.Truck,
                2 or 3 => VehicleType.Bike,
                _ => VehicleType.Car
            };
        }

        public int ComputeRank(int floor, int slotNumber, int floorCapacity)
        {
            // lower floors first, then lower slot number
            return (floor - 1) * floorCapacity + slotNumber;
        }
    }
}
