using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Seedwork.CQRS.Bus.Core;

namespace Seedwork.CQRS.Bus.IntegrationTests
{
    public class BusSerializer : IBusSerializer
    {
        public Task<T> Deserialize<T>(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return Task.FromResult(JsonConvert.DeserializeObject<T>(json));
        }

        public Task<byte[]> Serialize<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return Task.FromResult(Encoding.UTF8.GetBytes(json));
        }
    }
}