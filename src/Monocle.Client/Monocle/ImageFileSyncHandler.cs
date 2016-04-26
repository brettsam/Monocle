using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Xamarin.Forms;

namespace Monocle
{
    public class ImageFileSyncHandler : IFileSyncHandler
    {
        private readonly ImageManager imageManager;
        public ImageFileSyncHandler(ImageManager imageManager)
        {
            this.imageManager = imageManager;
        }

        public Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
        {
            IPlatform platform = DependencyService.Get<IPlatform>();
            return platform.GetFileDataSource(metadata);
        }

        public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
        {
            if (action == FileSynchronizationAction.Delete)
            {
                await FileHelper.DeleteLocalFileAsync(file);
            }
            else // Create or update. We're aggressively downloading all files.
            {
                await this.imageManager.DownloadFileAsync(file);
            }
        }
    }
}
