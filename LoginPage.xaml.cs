using Hmed.AppComponents;
using System;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hmed.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private LoginViewModel LoginViewModel => (BindingContext as LoginViewModel);
        public LoginPage(LoginViewModel loginViewModel)
        {
            BindingContext = loginViewModel;

            InitializeComponent();

            Usuario.Completed += Usuario_Completed;
            Senha.Completed += Senha_Completed;

            if (Device.RuntimePlatform == Device.iOS)
            {
                Senha.Unfocused += PosicionaEntryUnfocused;
                Usuario.Unfocused += PosicionaEntryUnfocused;

                Usuario.Focused += PosicionaEntryFocused;
                Senha.Focused += PosicionaEntryFocused;
            }
        }

        private void PosicionaEntryFocused(object sender, FocusEventArgs e)
        {
            Posicionar();
        }

        private void PosicionaEntryUnfocused(object sender, FocusEventArgs e)
        {
            Thread.Sleep(100);
            Posicionar();
        }

        private void Usuario_Completed(object sender, EventArgs e)
        {
            Senha.Focus();
        }
        private void Senha_Completed(object sender, EventArgs e)
        {
            LoginViewModel.EntrarExecute();
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                NavigationManager.Voltar();
            });
        }

        private void PageSizeChanged(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                if (Usuario.IsFocused || Senha.IsFocused) (BindingContext as LoginViewModel).OcultarBottom(true);
                else (BindingContext as LoginViewModel).OcultarBottom(false);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (LoginViewModel.ReabrirPopUp)
            {
                LoginViewModel.ReabrirPopUp = false;
                LoginViewModel.AbrirPopUp();
            }
            LoginViewModel.canExecute = true;
        }

        private void Posicionar()
        {

            if (!Usuario.IsFocused && !Senha.IsFocused) AbsoluteLayout.SetLayoutBounds(Stack, new Rectangle(0.5, 0.5, 1, 1));
            else AbsoluteLayout.SetLayoutBounds(Stack, new Rectangle(0.5, 0, 1, 0.59));

        }
    }
}