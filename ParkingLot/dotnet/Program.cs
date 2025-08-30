using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ParkingLot.ServiceLayer;

namespace ParkingLot
{
    class Program
    {
        static void Main(string[] args)
        {
            var startup = new Startup();
            var serviceProvider = startup.ServiceProvider;
            RunAsync(serviceProvider.GetRequiredService<IParkingLotFacade>()).GetAwaiter().GetResult();
        }

        private static async Task RunAsync(IParkingLotFacade parkingLotFacade)
        {
            string operations = @"
                create_parking_lot PR1234 2 6

                display PR1234 free_count CAR
                display PR1234 free_count BIKE
                display PR1234 free_count TRUCK
                display PR1234 free_slots CAR
                display PR1234 free_slots BIKE
                display PR1234 free_slots TRUCK
                display PR1234 occupied_slots CAR
                display PR1234 occupied_slots BIKE
                display PR1234 occupied_slots TRUCK

                park_vehicle PR1234 CAR KA-01-DB-1234 black
                park_vehicle PR1234 CAR KA-02-CB-1334 red
                park_vehicle PR1234 CAR KA-01-DB-1133 black
                park_vehicle PR1234 CAR KA-05-HJ-8432 white
                park_vehicle PR1234 CAR WB-45-HO-9032 white
                park_vehicle PR1234 CAR KA-01-DF-8230 black
                park_vehicle PR1234 CAR KA-21-HS-2347 red

                display PR1234 free_count CAR
                display PR1234 free_count BIKE
                display PR1234 free_count TRUCK

                unpark_vehicle PR1234_2_5
                unpark_vehicle PR1234_2_5
                unpark_vehicle PR1234_2_7

                display PR1234 free_count CAR
                display PR1234 free_count BIKE
                display PR1234 free_count TRUCK
                display PR1234 free_slots CAR
                display PR1234 free_slots BIKE
                display PR1234 free_slots TRUCK
                display PR1234 occupied_slots CAR
                display PR1234 occupied_slots BIKE
                display PR1234 occupied_slots TRUCK

                park_vehicle PR1234 BIKE KA-01-DB-1541 black
                park_vehicle PR1234 TRUCK KA-32-SJ-5389 orange
                park_vehicle PR1234 TRUCK KL-54-DN-4582 green
                park_vehicle PR1234 TRUCK KL-12-HF-4542 green

                display PR1234 free_count CAR
                display PR1234 free_count BIKE
                display PR1234 free_count TRUCK
                display PR1234 free_slots CAR
                display PR1234 free_slots BIKE
                display PR1234 free_slots TRUCK
                display PR1234 occupied_slots CAR
                display PR1234 occupied_slots BIKE
                display PR1234 occupied_slots TRUCK
                exit";

            string[] operationsArray = operations.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            foreach (var operation in operationsArray)
            {
                var result = await parkingLotFacade.Run(operation.Trim());
                Console.WriteLine(new string('_', 50));
                Console.WriteLine(operation.Trim());
                Console.WriteLine(new string('.', 50));
                Console.WriteLine(result);
            }
        }
    }
}