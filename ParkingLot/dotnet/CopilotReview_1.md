# Parking Lot Solution Review (dotnet)

Evaluator: GitHub Copilot

## Summary
A clean, layered C# implementation (Service/Business/Data/Entity) with DI and simple in‑memory stores. Core slot assignment logic is sound and meets ordering rules. However, the CLI contract and output format significantly diverge from the problem statement. As‑is, it would fail the provided test harness due to parsing and output mismatches. Concurrency is only partially addressed.

## Requirements coverage (from README)
- Create parking lot: Implemented (CreateLot). Output format does not match spec.
- Add floors and slots: Implicitly supported via CreateLot arguments; no separate commands (acceptable).
- Park vehicle: Implemented with correct slot selection order; returns ticket.
- Unpark vehicle: Implemented; frees slot; validates ticket.
- Display free_count/free_slots/occupied_slots: Implemented per floor; formatting mismatches (case/text).
- Vehicle types: Car/Bike/Truck supported; slot typing follows rule (1:Truck, 2-3:Bike, rest:Car).
- Ticket format: Uses <lot>_<floor>_<slot> (matches example); ticket id equals slot id.
- Input handling: Not compliant; Program uses a hard‑coded script, not stdin loop.
- Output contract: Not compliant; extra text and casing differences.

## Correctness vs I/O contract (blocking for automated tests)
- Command grammar mismatch:
  - Spec: `park_vehicle <vehicle_type> <reg_no> <color>`; Code expects `park_vehicle <lot_id> <vehicle_type> <reg_no> <color>`.
  - Spec: `display <display_type> <vehicle_type>`; Code expects `display <lot_id> <display_type> <vehicle_type>`.
  - Result: Provided sample inputs will not parse; and printed outputs won’t match expected.
- Output strings differ:
  - Create lot: Spec → `Created parking lot with 2 floors and 6 slots per floor`; Code → `Created parking lot PR1234 with ...` (extra lot id).
  - Display lines use enum `.ToString()` → `Car/Bike/Truck`; Spec requires upper case `CAR/BIKE/TRUCK`.
  - Unpark message: Spec → `Registration Number:`; Code → `Registration No:`.
  - Program prints separators and the command itself between results; spec expects only the result lines.
- Exit behavior: Spec says stop reading on `exit`; Code returns `"Exiting..."` as a string.

## Functional gaps & edge cases
- No stdin loop: Program does not read lines until `exit`. It runs a fixed script and echoes commands, which breaks judge I/O.
- Lot existence handling:
  - `park_vehicle` on a non‑existent lot returns `Parking Lot Full` (misleading) because it doesn’t verify lot presence; should error clearly or align with spec handling.
  - `display` correctly errors on unknown lot but surfaces exception text (not spec’d output).
- Empty lists formatting: For lines like `Occupied slots ...:` the spec shows a trailing colon and space with nothing after it. Current join prints nothing (acceptable), but keep consistency with space after colon.
- Sorting: Current creation order yields correct ascending slot numbers; still safer to sort explicitly when printing.

## Design & code quality
- Good
  - Clear layering (Service -> BL -> DL -> Entity), DI via `Microsoft.Extensions.DependencyInjection`.
  - Slot selection with a global rank across floors ensures correct ordering (lowest floor then lowest slot).
  - Entities are simple/immutable where appropriate; BL composes DL functions well.
- Can improve
  - Parsing is tightly coupled to current multi‑lot command shape; consider a parser that supports both spec and extended grammar.
  - Error handling returns exception type names to output; the judge expects only the specified lines.
  - `Ticket.Id == SlotId` works for the given spec, but constrains extensibility (e.g., time‑series uniqueness, pricing). Consider separate ticket id and slot id fields (still print the slot‑formatted id to conform to spec).

## Concurrency & data integrity
- Uses `ConcurrentDictionary` for stores (good), but `PriorityQueue<,>` is not thread‑safe. Calls like `TryDequeue/Enqueue` aren’t synchronized, risking race conditions.
- Suggestions:
  - Introduce a per‑(lotId, vehicleType) lock or `SemaphoreSlim` around priority queue operations.
  - Alternatively, maintain per‑floor ordered `ConcurrentQueue` lists or a lock‑free index plus CAS on slot status.
  - Add idempotency/validation on `FreeUpSlot` and ticket deletion flows; current behavior is reasonable but logging would help.

## Testability
- No unit tests. Add tests covering:
  - Full sample I/O transcript (happy path) asserting exact lines.
  - Edge cases: duplicate create, invalid ticket, full lot, unknown lot, mixed vehicle parking order.
  - Concurrency: parallel park/unpark on same vehicle type preserves ordering and consistency.

## Suggested fixes (priority ordered)
1) Align CLI contract and I/O exactly to spec
- Program: read from stdin line‑by‑line until `exit`; do not echo commands or add separators.
- Parser: support the spec grammar. If multi‑lot support is desired, add an optional variant but keep the original contract working. For example, default to the single created lot when commands omit `<lot_id>`.
- Output text: match strings precisely: casing (CAR/BIKE/TRUCK), `Registration Number`, and remove extra lot id from create message.
- Exit: stop processing without printing anything.

2) Harden behavior for invalid inputs
- Validate lot existence before parking; return appropriate spec‑aligned messages (or silence if spec prefers none).
- Make display outputs stable even if lot missing (either no output, or a clear spec‑aligned error policy).

3) Concurrency safety for slot index
- Protect priority queue operations with a per‑key lock; keep enqueue/dequeue + slot status updates atomic.
- Consider encapsulating slot allocation in DL to ensure single authority updates `_availableVehicleTypeSlotsOrderedIndex` and slot status.

4) Improve extensibility
- Extract `ISlotAssignmentStrategy` to encapsulate first‑available policy and vehicle type by slot mapping (1→Truck, 2–3→Bike, rest→Car).
- Keep ticket identity independent from slot id internally while still printing the slot‑formatted id.

5) Observability & tests
- Add unit tests for all display variants and exact text.
- Add simple logging for errors and unexpected states in DL/BL.

## Minor nits
- Remove unused `retryCount` parameter in `GetAvailableSlot` or implement a retry policy.
- Prefer explicit ordering (`OrderBy`) when printing free/occupied slots to avoid future regressions.
- Normalize output spacing after colons to match samples.

## Conclusion
The core data model and allocation logic are solid, but the CLI contract and output formatting diverge from the spec, which is a blocker for automated evaluation. Address the parsing/printing gaps first, then add concurrency safety and tests for a production‑grade submission.
