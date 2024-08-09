using Hmed.AppComponents;
using Hmed.AppComponents.Interfaces;
using Hmed.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hmed.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelecaoDeHospitaisPage : ContentPage
    {
        public SelecaoDeHospitaisPage()
        {
            BindingContext = new LoginViewModel();
            NavigationPage.SetBackButtonTitle(this, "");
            InitializeComponent();
        }
        private void HospitalLista_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                (BindingContext as LoginViewModel).AbrirHospitalExecute(e.Item as Hospital);
            }
        }

        private void ListView_Scrolled(object sender, ScrolledEventArgs e)
        {
            if (Device.iOS == Device.RuntimePlatform) DependencyService.Get<IKeyboardHelper>().HideKeyboard();
        }
    }
}