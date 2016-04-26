using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Server.Files;

namespace monocle_mobileService
{
    public class ContainerNameResolver : IContainerNameResolver
    {
        private const string ContainerName = "images-pre";

        public Task<string> GetFileContainerNameAsync(string tableName, string recordId, string fileName)
        {
            return Task.FromResult(ContainerName);
        }

        public Task<IEnumerable<string>> GetRecordContainerNames(string tableName, string recordId)
        {
            var containers = new[] { ContainerName };
            return Task.FromResult(containers.AsEnumerable());
        }
    }
}