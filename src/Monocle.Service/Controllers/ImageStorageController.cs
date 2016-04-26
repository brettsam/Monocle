using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.Azure.Mobile.Server.Files.Controllers;
using System.Linq;
using Monocle.Service.DataObjects;

namespace monocle_mobileService.Controllers
{
    public class ImageStorageController : StorageController<Image>
    {
        [HttpPost]
        [Route("tables/image/{id}/StorageToken")]
        public async Task<IHttpActionResult> PostStorageTokenRequest(string id, StorageTokenRequest value)
        {
            StorageToken token = await GetStorageTokenAsync(id, value, new ContainerNameResolver());
            return Ok(token);
        }

        [HttpGet]
        [Route("tables/image/{id}/MobileServiceFiles")]
        public async Task<IHttpActionResult> GetFiles(string id)
        {
            IEnumerable<MobileServiceFile> files = await GetRecordFilesAsync(id, new ContainerNameResolver());

            // filter down to the single file that we need.
            files = files.Where(f => f.Name.StartsWith(id));

            return Ok(files);
        }

        [HttpDelete]
        [Route("tables/image/{id}/MobileServiceFiles/{name}")]
        public Task Delete(string id, string name)
        {
            return DeleteFileAsync(id, name, new ContainerNameResolver());
        }
    }
}