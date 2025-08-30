using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParkingLot.Entity;
using ParkingLot.Entity.Enum;

namespace ParkingLot.DataLayer
{
    public class LotDL : ILotDL
    {
        private readonly ConcurrentDictionary<string, Lot> _lots;
        private readonly ConcurrentDictionary<string, Slot> _slots;
        private readonly ConcurrentDictionary<string, List<string>> _lotSlotsMapping;
        private readonly ConcurrentDictionary<(string, VehicleType), PriorityQueue<string, int>> _availableVehicleTypeSlotsOrderedIndex;
        private readonly Func<int, VehicleType> _getVehicleTypeBySlotStrategy;
        public LotDL()
        {
            _lots = new ConcurrentDictionary<string, Lot>();
            _slots = new ConcurrentDictionary<string, Slot>();
            _lotSlotsMapping = new ConcurrentDictionary<string, List<string>>();
            _availableVehicleTypeSlotsOrderedIndex = new ConcurrentDictionary<(string, VehicleType), PriorityQueue<string, int>>();

            _getVehicleTypeBySlotStrategy = slotNumber =>
            {
                return slotNumber switch
                {
                    1 => VehicleType.Truck,
                    2 or 3 => VehicleType.Bike,
                    _ => VehicleType.Car
                };
            };
        }

        public Task<bool> CreateLot(Lot lot)
        {
            if(!_lots.TryAdd(lot.Id, lot))
            {
                return Task.FromResult(false);
            }

            for(int i = 1; i <= lot.Floors; i++)
            {
                for (int j = 1; j <= lot.FloorCapacity; j++)
                {
                    VehicleType vehicleType = _getVehicleTypeBySlotStrategy(j);
                    var slot = new Slot(vehicleType, lot.Id, i, j, lot.FloorCapacity);
                    if (!TryAddSlotToLot(lot.Id, slot))
                    {
                        return Task.FromResult(false);
                    }
                }
            }

            return Task.FromResult(true);
        }

        private bool TryAddSlotToLot(string lotId, Slot slot)
        {
            if (!_lots.TryGetValue(lotId, out var lot))
            {
                return false;
            }

            if (lot.FloorCapacity < slot.SlotNumber)
            {
                return false;
            }

            if (lot.Floors < slot.Floor)
            {
                return false;
            }

            if (!_slots.TryAdd(slot.Id, slot))
            {
                return false;
            }

            _lotSlotsMapping.AddOrUpdate(lot.Id, new List<string> { slot.Id }, (key, oldValue) =>
            {
                oldValue.Add(slot.Id);
                return oldValue;
            });

            _availableVehicleTypeSlotsOrderedIndex.AddOrUpdate((lot.Id, slot.VehicleType),
                new PriorityQueue<string, int>(new[] { (slot.Id, slot.Rank) }),
                (key, oldValue) =>
                    {
                        oldValue.Enqueue(slot.Id, slot.Rank);
                        return oldValue;
                    }
            );

            return true;
        }

        public async Task<Lot> GetLotById(string lotId)
        {
            if (!_lots.TryGetValue(lotId, out var lot))
            {
                return null;
            }

            return await Task.FromResult(lot);
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
            _availableVehicleTypeSlotsOrderedIndex.AddOrUpdate((slot.LotId, slot.VehicleType),
                new PriorityQueue<string, int>([(slot.Id, slot.Rank)]),
                (key, oldValue) =>
                {
                    oldValue.Enqueue(slot.Id, slot.Rank);
                    return oldValue;
                }
            );

            return await Task.FromResult(true);
        }

        public async Task<Slot> GetAvailableSlot(string lotId, VehicleType vehicleType, int retryCount = 0)
        {
            if (!_availableVehicleTypeSlotsOrderedIndex.TryGetValue((lotId, vehicleType), out var availableSlots) || availableSlots.Count == 0)
            {
                return null;
            }

            if (!availableSlots.TryDequeue(out var slotId, out var _))
            {
                return null;
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

            var slots = slotIds.Select(id => _slots[id]).Where(slot => slot.VehicleType == vehicleType).ToList();
            return Task.FromResult(slots);
        }
    }
}