using System.Threading.Tasks;
using static Handlers.Delegates;

namespace Handlers.Interfaces
{
    public interface IDataHandler
    {
        event LogHandler OnStatusLog;
        Task<dynamic> Handle(dynamic data);
    }
}
