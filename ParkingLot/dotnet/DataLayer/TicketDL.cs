using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParkingLot.Entity;

namespace ParkingLot.DataLayer
{
    public class TicketDL : ITicketDL
    {
        // Store by internal id for uniqueness and a mapping from display id to internal id
        private readonly ConcurrentDictionary<Guid, Ticket> _ticketsByInternalId;
        private readonly ConcurrentDictionary<string, Guid> _displayToInternal;

        public TicketDL()
        {
            _ticketsByInternalId = new ConcurrentDictionary<Guid, Ticket>();
            _displayToInternal = new ConcurrentDictionary<string, Guid>();
        }

        public Task<bool> CreateTicket(Ticket ticket)
        {
            if (!_ticketsByInternalId.TryAdd(ticket.InternalId, ticket))
            {
                return Task.FromResult(false);
            }
            _displayToInternal[ticket.DisplayId] = ticket.InternalId;
            return Task.FromResult(true);
        }

        public Task<Ticket> DeleteTicket(string ticketDisplayId)
        {
            if (!_displayToInternal.TryRemove(ticketDisplayId, out var internalId))
            {
                return Task.FromResult<Ticket>(null);
            }
            if (!_ticketsByInternalId.TryGetValue(internalId, out var ticket))
            {
                return Task.FromResult<Ticket>(null);
            }
            ticket.CloseTicket();
            return Task.FromResult(ticket);
        }
    }
}