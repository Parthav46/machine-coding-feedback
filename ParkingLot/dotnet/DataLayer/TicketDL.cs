using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParkingLot.Entity;

namespace ParkingLot.DataLayer
{
    public class TicketDL : ITicketDL
    {
        private readonly ConcurrentDictionary<string, Ticket> _tickets;

        public TicketDL()
        {
            _tickets = new ConcurrentDictionary<string, Ticket>();
        }

        public Task<bool> CreateTicket(Ticket ticket)
        {
            if (!_tickets.TryAdd(ticket.Id, ticket))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<Ticket> DeleteTicket(string ticketId)
        {
            if (!_tickets.TryRemove(ticketId, out var ticket))
            {
                return Task.FromResult<Ticket>(null);
            }

            return Task.FromResult(ticket);
        }
    }
}