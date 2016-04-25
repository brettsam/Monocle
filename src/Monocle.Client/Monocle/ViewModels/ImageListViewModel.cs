using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Monocle.Views;
using Xamarin.Forms;

namespace Monocle
{
    public class ImageListViewModel : ViewModel
    {
        private ImageManager manager;
        private string newItemText = "";

        private ObservableCollection<ImageViewModel> images;
        private long pendingChanges;
        private bool isStatusBarVisible;

        public ObservableCollection<ImageViewModel> Images
        {
            get
            {
                return images;
            }
            set
            {
                images = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddItemCommand { get; set; }
        public ICommand DeleteItemCommand { get; set; }

        public string NewItemText
        {
            get
            {
                return newItemText;
            }

            set
            {
                newItemText = value;
                OnPropertyChanged();
            }
        }

        public long PendingChanges
        {
            get
            {
                return pendingChanges;
            }

            set
            {
                pendingChanges = value;
                OnPropertyChanged();
            }
        }

        public bool IsStatusBarVisible
        {
            get
            {
                return isStatusBarVisible;
            }

            set
            {
                isStatusBarVisible = value;
                OnPropertyChanged();
            }
        }

        public ImageListViewModel()
        {
            InitCommands();

            ImageManager.CreateAsync().ContinueWith(x =>
            {
                this.manager = x.Result;
                this.manager.MobileServiceClient.EventManager.Subscribe<StoreOperationCompletedEvent>(StoreOperationEventHandler);

                Device.BeginInvokeOnMainThread(async () => { await SyncItemsAsync(); });
            });
        }

        private async void StoreOperationEventHandler(StoreOperationCompletedEvent mobileServiceEvent)
        {
            await Task.Delay(500);
            PendingChanges = manager.MobileServiceClient.SyncContext.PendingOperations;
            IsStatusBarVisible = PendingChanges > 0;
        }

        private void InitCommands()
        {
            this.AddItemCommand = new DelegateCommand(AddItem);
            this.DeleteItemCommand = new DelegateCommand(DeleteItem);
        }

        private async Task LoadItems()
        {
            IEnumerable<Image> items = await manager.GetImagesAsync();
            Images = new ObservableCollection<ImageViewModel>();

            foreach (var i in items)
            {
                MobileServiceFile file = await this.manager.GetImageFileAsync(i);
                Images.Add(await ImageViewModel.CreateAsync(i, file, this.manager));
                Debug.WriteLine("Created view model for: " + i.Id);
            }
        }

        public async Task SyncItemsAsync()
        {
            await manager.SyncAsync();
            await LoadItems();
        }

        private async void AddItem(object data)
        {
            IPlatform mediaProvider = DependencyService.Get<IPlatform>();
            string sourceImagePath = await mediaProvider.TakePhotoAsync(App.UIContext);

            //var mediaPicker = new MediaPicker(App.UIContext);
            //var photo = await mediaPicker.TakePhotoAsync(new StoreCameraMediaOptions());

            if (sourceImagePath != null)
            {
                Image image = new Image();
                image.Id = Guid.NewGuid().ToString();

                MobileServiceFile file = await this.manager.AddImageFile(image, sourceImagePath);

                await manager.SaveTaskAsync(image);
                await LoadItems();
            }
        }

        async void DeleteItem(object data)
        {
            var viewModel = data as ImageViewModel;

            await manager.DeleteTaskAsync(viewModel.GetItem());
            await LoadItems();
        }   
    }
}
