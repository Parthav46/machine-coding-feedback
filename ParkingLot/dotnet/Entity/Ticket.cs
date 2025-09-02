using System;
using ParkingLot.Entity.Enum;

namespace ParkingLot.Entity
{
    public class Ticket
    {
        // New: internal unique identity decoupled from display/slot id
        public Guid InternalId { get; private set; } = Guid.NewGuid();

        // Slot-formatted id shown to users: <lot>_<floor>_<slot>
        public string DisplayId { get; private set; }

        public string SlotId { get; private set; }
        public string RegistrationNo { get; private set; }
        public string Color { get; private set; }
        public VehicleType VehicleType { get; private set; }
        public DateTime IssuedAt { get; private set; } = DateTime.Now;
        public TicketStatus Status { get; private set; } = TicketStatus.Active;

        public Ticket(string slotId, VehicleType vehicleType, string registrationNo, string color)
        {
            DisplayId = slotId;
            SlotId = slotId;
            VehicleType = vehicleType;
            RegistrationNo = registrationNo;
            Color = color;
        }

        public void CloseTicket()
        {
            Status = TicketStatus.Closed;
        }
    }
}