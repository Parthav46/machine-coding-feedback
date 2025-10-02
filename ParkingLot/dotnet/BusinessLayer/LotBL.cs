using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ParkingLot.DataLayer;
using ParkingLot.Entity;
using ParkingLot.Entity.Enum;

namespace ParkingLot.BusinessLayer
{
    public class LotBL : ILotBL
    {
        private readonly ILotDL _lotDL;

        public LotBL(ILotDL lotDL)
        {
            _lotDL = lotDL;
        }

        public async Task<bool> CreateLot(string name, int floors, int capacity)
        {
            Lot lot = new(name, floors, capacity);
            return await _lotDL.CreateLot(lot);
        }

        public async Task<bool> FreeUpSlot(string slotId)
        {
            return await _lotDL.FreeUpSlot(slotId);
        }

        public async Task<Slot> GetAvailableSlot(string lotId, VehicleType vehicleType)
        {
            return await _lotDL.GetAvailableSlot(lotId, vehicleType);
        }

        public async Task<string> GetStats(string lotId, MetricType metric, VehicleType vehicleType)
        {
            return metric switch
            {
                MetricType.FreeCount => await GetFreeCountMetric(lotId, vehicleType),
                MetricType.FreeSlots => await GetFreeSlotsMetric(lotId, vehicleType),
                MetricType.OccupiedSlots => await GetOccupiedSlotsMetric(lotId, vehicleType),
                _ => throw new ArgumentException("Invalid Metric Type")
            };
        }

        private async Task<string> GetFreeCountMetric(string lotId, VehicleType vehicleType)
        {
            var lot = await _lotDL.GetLotById(lotId);
            if (lot == null)
            {
                throw new ArgumentException("Lot not found");
            }

            var slots = await _lotDL.GetSlotsForVehicleType(lotId, vehicleType);
            int[] freeCount = new int[lot.Floors];
            foreach (var slot in slots)
            {
                if (slot.Status == SlotStatus.Available)
                {
                    freeCount[slot.Floor - 1]++;
                }
            }

            StringBuilder result = new();
            for (int i = 0; i < lot.Floors; i++)
            {
                result.AppendLine($"No. of free slots for {vehicleType.ToString()} on Floor {i + 1}: {freeCount[i]}");
            }
            if (result.Length > 0)
            {
                result.Length -= Environment.NewLine.Length; // Remove the last newline
            }

            return result.ToString();

        }

        private async Task<string> GetFreeSlotsMetric(string lotId, VehicleType vehicleType)
        {
            var lot = await _lotDL.GetLotById(lotId);
            if (lot == null)
            {
                throw new ArgumentException("Lot not found");
            }

            var slots = await _lotDL.GetSlotsForVehicleType(lotId, vehicleType);
            List<int>[] freeSlotsPerFloor = new List<int>[lot.Floors];
            for (int i = 0; i < lot.Floors; i++)
            {
                freeSlotsPerFloor[i] = new List<int>();
            }

            foreach (var slot in slots)
            {
                if (slot.Status == SlotStatus.Available)
                {
                    freeSlotsPerFloor[slot.Floor - 1].Add(slot.SlotNumber);
                }
            }
            StringBuilder result = new();
            for (int i = 0; i < freeSlotsPerFloor.Length; i++)
            {
                result.AppendLine($"Free slots for {vehicleType.ToString()} on Floor {i + 1}: {string.Join(",", freeSlotsPerFloor[i])}");
            }
            if (result.Length > 0)
            {
                result.Length -= Environment.NewLine.Length; // Remove the last newline
            }

            return result.ToString();

        }

        private async Task<string> GetOccupiedSlotsMetric(string lotId, VehicleType vehicleType)
        {
            var lot = await _lotDL.GetLotById(lotId);
            if (lot == null)
            {
                throw new ArgumentException("Lot not found");
            }

            var slots = await _lotDL.GetSlotsForVehicleType(lotId, vehicleType);
            List<int>[] occupiedSlotsPerFloor = new List<int>[lot.Floors];
            for (int i = 0; i < lot.Floors; i++)
            {
                occupiedSlotsPerFloor[i] = new List<int>();
            }

            foreach (var slot in slots)
            {
                if (slot.Status == SlotStatus.Occupied)
                {
                    occupiedSlotsPerFloor[slot.Floor - 1].Add(slot.SlotNumber);
                }
            }

            StringBuilder result = new();
            for (int i = 0; i < occupiedSlotsPerFloor.Length; i++)
            {
                result.AppendLine($"Occupied slots for {vehicleType.ToString()} on Floor {i + 1}: {string.Join(",", occupiedSlotsPerFloor[i])}");
            }
            if (result.Length > 0)
            {
                result.Length -= Environment.NewLine.Length; // Remove the last newline
            }

            return result.ToString();
        }
    }
}
