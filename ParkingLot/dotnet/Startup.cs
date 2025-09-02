using System;
using Microsoft.Extensions.DependencyInjection;
using ParkingLot.BusinessLayer;
using ParkingLot.DataLayer;
using ParkingLot.ServiceLayer;

namespace ParkingLot
{
    public class Startup
    {
        private readonly IServiceCollection _services;
        public IServiceProvider ServiceProvider;
        public Startup()
        {
            _services = new ServiceCollection();
            ConfigureServices(_services);
            ServiceProvider = _services.BuildServiceProvider();
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IParkingLotFacade, ParkingLotFacade>();
            services.AddScoped<ITicketBL, TicketBL>();
            services.AddScoped<ILotBL, LotBL>();
            services.AddSingleton<ITicketDL, TicketDL>();
            services.AddSingleton<ISlotAssignmentStrategy, FirstAvailableSlotAssignmentStrategy>();
            services.AddSingleton<ILotDL, LotDL>();
        }
    }
}