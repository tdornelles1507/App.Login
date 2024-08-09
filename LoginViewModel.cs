using Hmed.AppComponents;
using Hmed.AppComponents.Helper;
using Hmed.AppComponents.Interfaces;
using Hmed.Main;
using Hmed.Models;
using Hmed.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Hmed.Login
{
    public class LoginViewModel : ViewModelBase
    {
        #region Ocultar/Mostrar Password
        public Command AlternaPassword { get; set; }
        public bool OcultaPassword
        {
            get { return ocultaPassword; }
            set
            {
                SetProperty(ref ocultaPassword, value);
                Change(nameof(IconePassword));
            }
        }
        bool ocultaPassword;
        public string IconePassword => OcultaPassword ? "ic_hide_password" : "ic_show_password";

        internal void AjustaPagina(LoginPage loginPage, Page novaPage)
        {
            NavigationManager.StackInserirPaginaPosicao(loginPage, novaPage);
            NavigationManager.Voltar(false);
        }
        #endregion
        public bool MostraBottom { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public bool Bloqueio { get { return bloqueio; } set { SetProperty(ref bloqueio, value); } }
        private bool bloqueio;
        public bool LoginFalhou
        {
            get { return loginFalhou; }
            set
            {
                SetProperty(ref loginFalhou, value);
                Change(nameof(EntryBorderColor));
            }
        }
        private string titulo;
        public string Titulo { get { return titulo; } set { SetProperty(ref titulo, value); } }

        private bool loginFalhou;

        internal void OcultarBottom(bool isFocused)
        {
            MostraBottom = !isFocused;
            Change(nameof(MostraBottom));
        }
        public ICommand Entrar { get; set; }
        public ICommand Voltar { get; set; }
        public ICommand AceitarTermos { get; set; }
        public ICommand AbrirTermosUso { get; set; }
        public ICommand AbrirPoliticaPrivacidade { get; set; }
        public ICommand AlternaDemonstracao { get; set; }
        public string Version => Configuration.VersaoAtual;

        private bool mostraDemonstracao;
        public bool MostraDemonstracao
        {
            get { return mostraDemonstracao; }
            set { SetProperty(ref mostraDemonstracao, value); Change(nameof(Hospitais)); }
        }
        private ObservableCollection<Hospital> hospitais;
        public ObservableCollection<Hospital> Hospitais
        {
            get
            {
                if (hospitais == null)
                    return new ObservableCollection<Hospital>();
                if (!string.IsNullOrEmpty(Pesquisa))
                    return new ObservableCollection<Hospital>(hospitais.Where(p => p.Nome.ToLower().RemoverAcentos().Contains(Pesquisa.ToLower().RemoverAcentos()) && p.Demonstracao == MostraDemonstracao));
                return new ObservableCollection<Hospital>(hospitais.Where(p => p.Demonstracao == MostraDemonstracao));
            }
            set
            {
                SetProperty(ref hospitais, value);
            }
        }
        public Color EntryBorderColor => LoginFalhou ? Color.OrangeRed : Color.Transparent;
        private bool mostrarAceitarTermos;
        public bool MostrarAceitarTermos
        {
            get
            {
                if (Carregado) return false;
                return mostrarAceitarTermos;
            }
            set
            {
                SetProperty(ref mostrarAceitarTermos, value);
            }
        }
        public bool HabilitarBotao
        {
            get
            {
                return Checkbox == true ? true : false;
            }
        }
        private bool _Checkbox;
        public bool Checkbox
        {
            get
            {
                return _Checkbox;
            }
            set
            {
                SetProperty(ref _Checkbox, value);
                Change(nameof(HabilitarBotao));
            }
        }
        private Hospital _hospitalSelecionado;
        public Hospital HospitalSelecionado
        {
            get { return _hospitalSelecionado; }
            set
            {
                SetProperty(ref _hospitalSelecionado, value);
                Configuration.UrlHost = _hospitalSelecionado.Host;
                Change(nameof(CaminhoLogo));
            }
        }
        public Uri CaminhoLogo
        {
            get
            {
                if (_hospitalSelecionado == null) return null;
                return new Uri(_hospitalSelecionado.CaminhoLogo);
            }
        }
        public LoginViewModel()
        {
            Titulo = "Instituições";
            AlternaPassword = new Command(AlternaPasswordExecute);
            Entrar = new Command(EntrarExecute);
            Voltar = new Command(VoltarExecute);
            AceitarTermos = new Command(AceitarTermosExecute);
            AbrirTermosUso = new Command(AbrirTermosUsoExecute);
            AbrirPoliticaPrivacidade = new Command(AbrirPoliticaPrivacidadeExecute);
            AlternaDemonstracao = new Command(AlternaDemonstracaoExecute);
            canExecute = true;
            OcultaPassword = true;
            MostraBottom = true;

            Task.Run(async () =>
            {
                CarregandoLista = true;
                Hospitais = new ObservableCollection<Hospital>(await HospitaisService.Instancia.GetHospitais());
                CarregarUberabaViaLink(Hospitais);
                Change(nameof(Hospitais));
                CarregandoLista = false;
            });
        }

        private async void CarregarUberabaViaLink(ObservableCollection<Hospital> hospitaluberaba)
        {
            var deeplink = await SecureStorage.GetAsync("deeplink");
            if (!string.IsNullOrEmpty(deeplink))
            {
                if (hospitaluberaba.Count > 0)
                    foreach (var item in hospitaluberaba)
                    {
                        if (item.Id == 116)
                        {
                            AbrirHospitalExecute(item);
                            SecureStorage.Remove("deeplink");
                        }

                    }
            }
        }
        private void AlternaPasswordExecute() { OcultaPassword = !OcultaPassword; }
        public void VoltarExecute()
        {
            Voltar.CanExecute(false);
            canExecute = true;
            NavigationManager.Voltar();
        }
        public void EntrarExecute()
        {
            if (Bloqueio)
            {
                NavigationManager.DisplayAlert("Ops", HospitalSelecionado.MensagemBloqueio, "Ok").ConfigureAwait(false);
                LoginFalhou = true;
                return;
            }

            if (VerificaLoginSenhaVazios())
            {
                LoginFalhou = true;
                return;
            }

            CarregandoLista = true;
            LoginFalhou = false;

            Task.Run(async () =>
            {
                try
                {
                    UsuarioAcesso usuarioAcesso = new UsuarioAcesso()
                    {
                        Login = Usuario.ToLower(),
                        Senha = Senha,
                        IdHospital = HospitalSelecionado.Id,
                        TokenFirebase = Configuration.FirebaseToken,
                        Plataforma = Device.RuntimePlatform,
                        Versao = Configuration.FileVersion,
                        IdAplicativo = Configuration.IdAplicativo
                    };

                    UsuarioAcesso token = await LoginService.GetToken(usuarioAcesso);

                    if (token != null && !token.Token.IsEmptyOrWhiteSpace())
                    {
                        Configuration.Token = token.Token;
                        Configuration.Usuario = Usuario.ToLower();
                        Configuration.Hospital = _hospitalSelecionado.Nome;
                        Configuration.IdHospital = _hospitalSelecionado.Id;                        
                        Configuration.LogoHome = _hospitalSelecionado.CaminhoLogo;
                        //USUARIO
                        await UsuarioService.Instancia.MedicoLogado();
                        //PARAMETROS
                        await ParametroService.Instancia.Parametros();

                        await TemaService.Instancia.GetTema().ContinueWith(x =>
                        {
                            if (x.Result != null)
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    App.AlterarCoresApp(x.Result.Cor1, x.Result.Cor2, x.Result.ImagemBackgroundAzul);
                                    DependencyService.Get<IToolbarManager>().AtualizaBackgroundImage();
                                });
                            }
                        });

                        VerificarAceitoTermos();
                    }
                    else
                    {
                        LoginFalhou = true;
                        CarregandoLista = false;
                    }
                }
                catch (Exception ex)
                {
                    CarregandoLista = false;
                    throw ex;
                }
            });
        }
        private bool VerificaLoginSenhaVazios() => string.IsNullOrEmpty(Usuario) || string.IsNullOrEmpty(Senha);

        public string Pesquisa
        {
            get { return _pesquisa; }
            set
            {
                _pesquisa = value;
                if (Hospitais == null) return;

                SetProperty(ref _pesquisa, value);
                Change(nameof(Hospitais));
                Change(nameof(NaoTemHospitais));
            }
        }
        private string _pesquisa;
        public bool NaoTemHospitais
        {
            get
            {
                if (Hospitais == null) return false;
                return !Hospitais.Any();
            }
        }
        private void VerificarAceitoTermos()
        {
            TermosService.Instancia.VerificarAceito().ContinueWith(x =>
            {
                Configuration.PoliticaPrivacidade = x.Result.UrlPolitica;
                Configuration.TermosUso = x.Result.UrlTermos;

                if (x.Result.IsAceito)
                {
                    Configuration.IsAceitoTermosUso = x.Result.IsAceito;
                    AbrirMainPage();
                }
                else
                {
                    AbrirPopUp();
                }
            });
        }
        public bool ReabrirPopUp;
        public void AbrirPopUp()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                NavigationManager.MostrarModal(new TermosPoliticaPopUp(this)).ConfigureAwait(true);
            });
        }
        private void AbrirPoliticaPrivacidadeExecute()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                NavigationManager.FecharModal();
            });
            NavigationManager.AbrePaginaWeb("Política de Privacidade", Configuration.PoliticaPrivacidade, true);
            ReabrirPopUp = true;
        }
        private void AbrirTermosUsoExecute()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                NavigationManager.FecharModal();
            });
            NavigationManager.AbrePaginaWeb("Termos de Uso", Configuration.TermosUso, true);
            ReabrirPopUp = true;
        }
        private async void AceitarTermosExecute()
        {
            NavigationManager.FecharModal();
            CarregandoLista = true;
            await TermosService.Instancia.GravarAceito().ConfigureAwait(false);
            AbrirMainPage();
        }
        public bool canExecute { get; set; }
        public void AbrirHospitalExecute(Hospital phospital)
        {
            if (canExecute)
            {
                canExecute = false;
                LoginFalhou = false;
                HospitalSelecionado = phospital;
                Usuario = string.Empty;
                Senha = string.Empty;
                Bloqueio = phospital.Bloqueado;
                if (Bloqueio)
                {
                    Device.InvokeOnMainThreadAsync(async () => await NavigationManager.DisplayAlert("Ops", phospital.MensagemBloqueio, "Ok"));
                    canExecute = true;
                }
                else NavigationManager.EmpilharNavegacao(new LoginPage(this));
            }
        }
        public void AlternaDemonstracaoExecute()
        {
            MostraDemonstracao = !MostraDemonstracao;
            if (MostraDemonstracao)
            {
                Titulo = "Instituições Demonstração";
            }
            else
            {
                Titulo = "Instituições";
            }
        }

        //TODO
        private void AbrirMainPage()
        {
            CarregandoLista = true;

            Device.BeginInvokeOnMainThread(() =>
            {
                IFireBaseAnalyticsService service = DependencyService.Get<IFireBaseAnalyticsService>();
                service?.SetUserId($"{Configuration.IdHospital}|{Configuration.Usuario}");

                if (Device.RuntimePlatform.Equals(Device.Android))
                    DependencyService.Get<IScreenshotBlock>().BloqueioScreenshot();
                Application.Current.MainPage = new MainPage();
                CarregandoLista = false;
            });

            CarregandoLista = false;
        }
    }
}
