using ParkingLot.Entity.Enum;

namespace ParkingLot.BusinessLayer
{
    public interface ISlotAssignmentStrategy
    {
        VehicleType ClassifyBySlotNumber(int slotNumber);

        int ComputeRank(int floor, int slotNumber, int floorCapacity);
    }
}
