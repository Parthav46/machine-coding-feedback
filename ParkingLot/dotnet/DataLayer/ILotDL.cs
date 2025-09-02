using System.Collections.Generic;
using System.Threading.Tasks;
using ParkingLot.Entity;
using ParkingLot.Entity.Enum;

namespace ParkingLot.DataLayer
{
    public interface ILotDL
    {
        Task<bool> CreateLot(Lot lot);
        Task<Lot> GetLotById(string lotId);
        Task<Slot> GetAvailableSlot(string lotId, VehicleType vehicleType);
        Task<bool> FreeUpSlot(string slotId);
        Task<List<Slot>> GetSlotsForVehicleType(string lotId, VehicleType vehicleType);
    }
}