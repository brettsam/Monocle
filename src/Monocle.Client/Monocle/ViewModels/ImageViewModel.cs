using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files;
using PCLStorage;

namespace Monocle
{
    public class ImageViewModel : ViewModel
    {
        private Image image;
        private string uri;
        private ImageManager itemManager;

        private ImageViewModel()
        {
        }

        public string Id
        {
            get { return image.Id; }
        }

        public string Uri
        {
            get

            {
                return this.uri;
            }
            set
            {
                if (string.Compare(this.uri, value) != 0)
                {
                    this.uri = value;
                    OnPropertyChanged();
                }
            }
        }

        public static async Task<ImageViewModel> CreateAsync(Image image, MobileServiceFile file, ImageManager imageManager)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }

            if (imageManager == null)
            {
                throw new ArgumentNullException("imageManager");
            }

            ImageViewModel result = new ImageViewModel();

            result.image = image;
            string localPath = await FileHelper.GetLocalFilePathAsync(image.Id, file.Name);
            result.uri = Path.Combine(FileSystem.Current.LocalStorage.Path, localPath);
            result.itemManager = imageManager;

            return result;
        }

        internal Image GetItem()
        {
            return this.image;
        }
    }
}