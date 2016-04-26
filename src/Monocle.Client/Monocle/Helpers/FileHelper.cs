using System.IO;
using System.Threading.Tasks;
using PCLStorage;
using Xamarin.Forms;

namespace Monocle
{
    public class FileHelper
    {
        public static async Task<string> CopyImageFileAsync(string itemId, string filePath)
        {
            IFolder localStorage = FileSystem.Current.LocalStorage;

            string fileName = Path.GetFileName(filePath);
            string targetPath = await GetLocalFilePathAsync(itemId, fileName);

            var sourceFile = await FileSystem.Current.GetFileFromPathAsync(filePath);

            var targetFile = await localStorage.CreateFileAsync(targetPath, CreationCollisionOption.ReplaceExisting);

            using (var targetStream = await targetFile.OpenAsync(FileAccess.ReadAndWrite))
            {
                using (var sourceStream = await sourceFile.OpenAsync(FileAccess.Read))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }
            }

            return targetPath;
        }

        // Return a path like 'images/{imageId}.jpg'
        public static async Task<string> GetLocalFilePathAsync(string itemId, string fileName)
        {
            IPlatform platform = DependencyService.Get<IPlatform>();
            var extension = Path.GetExtension(fileName);
            string recordFilesPath = Path.Combine(await platform.GetImageFilesPathAsync(), $"{itemId}{extension}");

            return recordFilesPath;
        }

        public static async Task DeleteLocalFileAsync(Microsoft.WindowsAzure.MobileServices.Files.MobileServiceFile fileName)
        {
            string localPath = await GetLocalFilePathAsync(fileName.ParentId, fileName.Name);
            var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(localPath);

            if (checkExists == ExistenceCheckResult.FileExists)
            {
                var file = await FileSystem.Current.LocalStorage.GetFileAsync(localPath);
                await file.DeleteAsync();
            }
        }
    }
}
