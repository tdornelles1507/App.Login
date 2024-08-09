using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hmed.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TermosPoliticaPopUp : PopupPage
    {
        public TermosPoliticaPopUp(LoginViewModel loginViewModel)
        {
            InitializeComponent();
            BindingContext = loginViewModel;
        }

        protected override void OnDisappearing()
        {
            (BindingContext as LoginViewModel).CarregandoLista = false;
        }
    }
}