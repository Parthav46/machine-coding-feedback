using System.Threading.Tasks;

namespace ParkingLot.ServiceLayer
{
    public interface IParkingLotFacade
    {
        Task<string> Run(string instruction);
    }
}