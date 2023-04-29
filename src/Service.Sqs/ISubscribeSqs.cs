using System.Threading;
using System.Threading.Tasks;

namespace Service.Sqs;

public interface ISubscribeSqs<T>
{
    Task HandleAsync(T dto);
}
