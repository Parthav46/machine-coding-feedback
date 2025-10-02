using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParkingLot.BusinessLayer;
using ParkingLot.Entity;
using ParkingLot.Entity.Enum;

namespace ParkingLot.DataLayer
{
    public class LotDL : ILotDL
    {
        private readonly ConcurrentDictionary<string, Lot> _lots;
        private readonly ConcurrentDictionary<string, Slot> _slots;
        private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _lotSlotsMapping;
        private readonly ConcurrentDictionary<(string, VehicleType), PriorityQueue<string, int>> _availableVehicleTypeSlotsOrderedIndex;
        private readonly ConcurrentDictionary<(string, VehicleType), SemaphoreSlim> _pqLocks;

        private readonly ISlotAssignmentStrategy _slotStrategy;

        public LotDL(ISlotAssignmentStrategy slotStrategy)
        {
            _slotStrategy = slotStrategy;
            _lots = new ConcurrentDictionary<string, Lot>();
            _slots = new ConcurrentDictionary<string, Slot>();
            _lotSlotsMapping = new ConcurrentDictionary<string, ConcurrentBag<string>>();
            _availableVehicleTypeSlotsOrderedIndex = new ConcurrentDictionary<(string, VehicleType), PriorityQueue<string, int>>();
            _pqLocks = new ConcurrentDictionary<(string, VehicleType), SemaphoreSlim>();
        }

        public async Task<bool> CreateLot(Lot lot)
        {
            if (!_lots.TryAdd(lot.Id, lot))
            {
                return false;
            }

        for (int floor = 1; floor <= lot.Floors; floor++)
            {
                for (int slotNumber = 1; slotNumber <= lot.FloorCapacity; slotNumber++)
                {
                    var vehicleType = _slotStrategy.ClassifyBySlotNumber(slotNumber);
                    var rank = _slotStrategy.ComputeRank(floor, slotNumber, lot.FloorCapacity);
                    var slot = new Slot(vehicleType, lot.Id, floor, slotNumber, rank);
                    if (!await TryAddSlotToLot(lot.Id, slot))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private async Task<bool> TryAddSlotToLot(string lotId, Slot slot)
        {
            if (!_lots.TryGetValue(lotId, out var lot))
            {
                return false;
            }

            if (lot.FloorCapacity < slot.SlotNumber || lot.Floors < slot.Floor)
            {
                return false;
            }

            if (!_slots.TryAdd(slot.Id, slot))
            {
                return false;
            }

            _lotSlotsMapping.AddOrUpdate(lot.Id,
                _ => new ConcurrentBag<string>(new[] { slot.Id }),
                (_, bag) => { bag.Add(slot.Id); return bag; });

            var key = (lot.Id, slot.VehicleType);
            var pq = _availableVehicleTypeSlotsOrderedIndex.GetOrAdd(key, _ => new PriorityQueue<string, int>());
            var sem = _pqLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            try
            {
                pq.Enqueue(slot.Id, slot.Rank);
            }
            finally
            {
                sem.Release();
            }

            return true;
        }

        public Task<Lot> GetLotById(string lotId)
        {
            _lots.TryGetValue(lotId, out var lot);
            return Task.FromResult(lot);
        }

        public async Task<bool> FreeUpSlot(string slotId)
        {
            if (!_slots.TryGetValue(slotId, out var slot))
            {
                return false;
            }

            if (slot.Status == SlotStatus.Available)
            {
                return false;
            }

            slot.RemoveVehicle();

            var key = (slot.LotId, slot.VehicleType);
            var pq = _availableVehicleTypeSlotsOrderedIndex.GetOrAdd(key, _ => new PriorityQueue<string, int>());
            var sem = _pqLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            try
            {
                pq.Enqueue(slot.Id, slot.Rank);
            }
            finally
            {
                sem.Release();
            }

            return true;
        }

        public async Task<Slot> GetAvailableSlot(string lotId, VehicleType vehicleType)
        {
            if (!_availableVehicleTypeSlotsOrderedIndex.TryGetValue((lotId, vehicleType), out var pq))
            {
                return null;
            }

            var key = (lotId, vehicleType);
            var sem = _pqLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            string slotId;
            await sem.WaitAsync();
            try
            {
                if (!pq.TryDequeue(out slotId, out _))
                {
                    return null;
                }
            }
            finally
            {
                sem.Release();
            }

            if (!_slots.TryGetValue(slotId, out var slot))
            {
                return null;
            }

            slot.ParkVehicle();
            return await Task.FromResult(slot);
        }

        public Task<List<Slot>> GetSlotsForVehicleType(string lotId, VehicleType vehicleType)
        {
            if (!_lotSlotsMapping.TryGetValue(lotId, out var slotIds))
            {
                return Task.FromResult(null as List<Slot>);
            }

            var slots = slotIds
                .Select(id => _slots[id])
                .Where(s => s.VehicleType == vehicleType)
                .ToList();
            return Task.FromResult(slots);
        }
    }
}