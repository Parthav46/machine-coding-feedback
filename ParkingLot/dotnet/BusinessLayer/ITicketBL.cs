using System.Threading.Tasks;
using ParkingLot.Entity;
using ParkingLot.Entity.Enum;

namespace ParkingLot.BusinessLayer
{
    public interface ITicketBL
    {
        Task<string> ParkVehicle(string lotId, VehicleType vehicle, string registrationNo, string color);
        Task<Ticket> UnparkVehicle(string ticketId);
    }
}