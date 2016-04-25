using Monocle.Views;
using Xamarin.Forms;

namespace Monocle
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new ImageList(new ImageListViewModel()));
        }

        public static object UIContext { get; set; }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}