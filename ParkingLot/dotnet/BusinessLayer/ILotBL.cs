using System.Threading.Tasks;
using ParkingLot.Entity;
using ParkingLot.Entity.Enum;

namespace ParkingLot.BusinessLayer
{
    public interface ILotBL
    {
        Task<bool> CreateLot(string name, int floors, int capacity);
        Task<Slot> GetAvailableSlot(string lotId, VehicleType vehicleType);
        Task<bool> FreeUpSlot(string slotId);
        Task<string> GetStats(string lotId, MetricType metric, VehicleType vehicleType);
    }
}