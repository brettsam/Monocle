using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using PCLStorage;
using Xamarin.Forms;

namespace Monocle
{
    public class ImageManager
    {
        MobileServiceClient client;
        IMobileServiceSyncTable<Image> imageTable;

        private ImageManager() { }

        public IMobileServiceClient MobileServiceClient
        {
            get
            {
                return this.client;
            }
        }

        public static async Task<ImageManager> CreateAsync()
        {
            var result = new ImageManager();
            result.client = new MobileServiceClient(Constants.ApplicationURL);

            var db = await FileSystem.Current.LocalStorage.GetFileAsync("localstore.db");
            await db.DeleteAsync();

            var store = new MobileServiceSQLiteStore("localstore.db");
            store.DefineTable<Image>();

            // Initialize file sync
            result.client.InitializeFileSyncContext(new ImageFileSyncHandler(result), store);

            // Initialize the SyncContext using the default IMobileServiceSyncHandler
            await result.client.SyncContext.InitializeAsync(store, StoreTrackingOptions.NotifyLocalAndServerOperations);

            result.imageTable = result.client.GetSyncTable<Image>();

            await result.imageTable.PurgeAsync(true);

            return result;
        }

        public async Task<MobileServiceFile> GetImageFileAsync(Image image)
        {
            var files = await this.imageTable.GetFilesAsync(image);
            return files.SingleOrDefault();
        }

        internal async Task DownloadFileAsync(MobileServiceFile file)
        {
            var todoItem = await imageTable.LookupAsync(file.ParentId);
            Debug.WriteLine("++ Downloading file: " + todoItem.Id);

            IPlatform platform = DependencyService.Get<IPlatform>();

            string filePath = await FileHelper.GetLocalFilePathAsync(file.ParentId, file.Name);
            await platform.DownloadFileAsync(this.imageTable, file, filePath);
        }

        internal async Task<IEnumerable<Image>> GetImagesAsync()
        {
            try
            {
                return await imageTable.OrderByDescending(item => item.CreatedAt).ToListAsync();
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
            return null;
        }

        internal async Task SyncAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                // await this.client.SyncContext.PushAsync();

                // FILES: Push file changes
                await this.imageTable.PushFileChangesAsync();

                // FILES: Automatic pull
                // A normal pull will automatically process new/modified/deleted files, engaging the file sync handler
                await this.imageTable.PullAsync("todoItems", this.imageTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }
                }
            }
        }

        public async Task<MobileServiceFile> AddImageFile(Image image, string sourceImagePath)
        {
            string targetPath = await FileHelper.CopyImageFileAsync(image.Id, sourceImagePath);

            // FILES: Creating/Adding file
            MobileServiceFile file = await this.imageTable.AddFileAsync(image, Path.GetFileName(targetPath));

            // "Touch" the record to mark it as updated
            // no longer required as of beta2
            //await this.todoTable.UpdateAsync(todoItem);

            return file;
        }

        internal async Task SaveTaskAsync(Image item)
        {
            await imageTable.InsertAsync(item);
        }

        internal async Task DeleteTaskAsync(Image item)
        {
            try
            {
                //TodoViewModel.TodoItems.Remove(item);
                await imageTable.DeleteAsync(item);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
        }
    }
}
