using System;
using System.Threading.Tasks;
using ParkingLot.BusinessLayer;
using ParkingLot.Entity.Enum;

namespace ParkingLot.ServiceLayer
{
    public class ParkingLotFacade : IParkingLotFacade
    {
        private readonly ITicketBL _ticketBL;
        private readonly ILotBL _lotBL;
        public ParkingLotFacade(ITicketBL ticketBL, ILotBL lotBL)
        {
            _ticketBL = ticketBL;
            _lotBL = lotBL;
        }

        public async Task<string> Run(string instruction)
        {
            try
            {
                var operationFunc = ParseInstruction(instruction);
                return await operationFunc();
            }
            catch (Exception ex)
            {
                return $"{ex.GetType().Name}: {ex.Message}";
            }
        }

        private Func<Task<string>> ParseInstruction(string instruction)
        {
            var parts = instruction.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                throw new ArgumentException("Invalid instruction");
            }

            var operation = parts[0].ToLower() switch
            {
                "create_parking_lot" => Operation.CreateParkingLot,
                "park_vehicle" => Operation.ParkVehicle,
                "unpark_vehicle" => Operation.UnparkVehicle,
                "display" => Operation.Display,
                "exit" => Operation.Exit,
                _ => throw new ArgumentException("Unknown operation")
            };

            return operation switch
            {
                Operation.CreateParkingLot => () => CreateLot(parts[1], int.Parse(parts[2]), int.Parse(parts[3])),
                Operation.ParkVehicle => () => ParkVehicle(parts[1], parts[2], parts[3], parts[4]),
                Operation.UnparkVehicle => () => UnparkVehicle(parts[1]),
                Operation.Display => () => DisplayStats(parts[1], parts[2], parts[3]),
                Operation.Exit => () => Task.FromResult("Exiting..."),
                _ => throw new ArgumentException("Unknown operation")
            };
        }

        private async Task<string> CreateLot(string name, int floors, int capacity)
        {
            var success = await _lotBL.CreateLot(name, floors, capacity);
            if (!success)
            {
                return "Failed to create parking lot";
            }
            return $"Created parking lot {name} with {floors} floors and {capacity} slots per floor";
        }

        private async Task<string> ParkVehicle(string lotId, string vehicleType, string registrationNo, string color)
        {
            VehicleType vehicle = vehicleType.ToLower() switch
            {
                "car" => VehicleType.Car,
                "bike" => VehicleType.Bike,
                "truck" => VehicleType.Truck,
                _ => throw new ArgumentException("Invalid Vehicle Type")
            };

            var ticketId = await _ticketBL.ParkVehicle(lotId, vehicle, registrationNo, color);
            if (string.IsNullOrEmpty(ticketId))
            {
                return "Parking Lot Full";
            }

            return $"Parked vehicle. Ticket ID: {ticketId}";
        }

        private async Task<string> UnparkVehicle(string ticketId)
        {
            var ticket = await _ticketBL.UnparkVehicle(ticketId);
            if (ticket == null)
            {
                return "Invalid Ticket";
            }
            return $"Unparked vehicle with Registration No: {ticket.RegistrationNo} and Color: {ticket.Color}";
        }

        private async Task<string> DisplayStats(string lotId, string metric, string vehicle)
        {
            MetricType metricType = metric.ToLower() switch
            {
                "free_count" => MetricType.FreeCount,
                "free_slots" => MetricType.FreeSlots,
                "occupied_slots" => MetricType.OccupiedSlots,
                _ => throw new ArgumentException("Invalid Metric Type")
            };

            VehicleType vehicleType = vehicle.ToLower() switch
            {
                "car" => VehicleType.Car,
                "bike" => VehicleType.Bike,
                "truck" => VehicleType.Truck,
                _ => throw new ArgumentException("Invalid Vehicle Type")
            };

            return await _lotBL.GetStats(lotId, metricType, vehicleType);
        }
    }
}