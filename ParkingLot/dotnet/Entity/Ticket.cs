using System;
using ParkingLot.Entity.Enum;

namespace ParkingLot.Entity
{
    public class Ticket
    {
        public string Id { get; private set; }
        public string SlotId { get; private set; }
        public string RegistrationNo { get; private set; }
        public string Color { get; private set; }
        public VehicleType VehicleType { get; private set; }
        public DateTime IssuedAt { get; private set; } = DateTime.Now;

        public Ticket(string slotId, VehicleType vehicleType, string registrationNo, string color)
        {
            Id = slotId;
            VehicleType = vehicleType;
            SlotId = slotId;
            RegistrationNo = registrationNo;
            Color = color;
        }
    }
}