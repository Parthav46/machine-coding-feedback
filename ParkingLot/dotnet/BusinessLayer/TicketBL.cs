using System;
using System.Threading.Tasks;
using ParkingLot.DataLayer;
using ParkingLot.Entity;
using ParkingLot.Entity.Enum;

namespace ParkingLot.BusinessLayer
{
    public class TicketBL : ITicketBL
    {
        private readonly ITicketDL _ticketDL;
        private readonly ILotBL _lotBL;

        public TicketBL(ITicketDL ticketDL, ILotBL lotBL)
        {
            _ticketDL = ticketDL;
            _lotBL = lotBL;
        }

        public async Task<string> ParkVehicle(string lotId, VehicleType vehicle, string registrationNo, string color)
        {
            var slot = await _lotBL.GetAvailableSlot(lotId, vehicle);
            if (slot == null)
            {
                return string.Empty;
            }

            // Create a ticket and save it using _ticketDL
            var ticket = new Ticket(slot.Id, vehicle, registrationNo, color);

            var success = await _ticketDL.CreateTicket(ticket);
            if (!success)
            {
                // Revert blocked slot
                await _lotBL.FreeUpSlot(slot.Id);
                throw new Exception("Failed to create ticket");
            }

            return ticket.Id;
        }

        public async Task<Ticket> UnparkVehicle(string ticketId)
        {
            var ticket = await _ticketDL.DeleteTicket(ticketId);
            if (ticket == null)
            {
                return null;
            }

            await _lotBL.FreeUpSlot(ticketId);
            return ticket;
        }
    }
}