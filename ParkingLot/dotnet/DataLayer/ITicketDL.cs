using System.Threading.Tasks;
using ParkingLot.Entity;

namespace ParkingLot.DataLayer
{
    public interface ITicketDL
    {
        Task<bool> CreateTicket(Ticket ticket);
        Task<Ticket> DeleteTicket(string ticketId);
    }
}