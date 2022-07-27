using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.IO;

namespace LightenceClient
{
    /// <summary>
    /// Logika interakcji dla klasy LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        //Boolean RegWindowOpened = false;

        public LoginWindow()
        {
            // create config file if not exists
            if (!File.Exists(Constants.configFileName)) Settings.SaveSettings();
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailField.Text.Trim().TrimEnd();
            string password = PasswordField.Password;

            if (email == "")
            {
                ErrorMsgBlock.Text = "No login input";
                return;
            }

            if (!EmailChecker.IsValid(email))
            {
                ErrorMsgBlock.Text = "Wrong email format";
                return;
            }

            if (password == "")
            {
                ErrorMsgBlock.Text = "No password input";
                return;
            }

            // miejsce na funkcje logowania
            var response = await HttpClientManager.LoginUserAsync(email, password);

            var msg = JObject.Parse(await response.Content.ReadAsStringAsync());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // jak jest dobrze to to :::
                JWToken.Token = msg["content"]["token"].ToString();
                // user profile initialization
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(JWToken.Token);
                Constants.currentUser.Email = email;
                Constants.currentUser.FirstLastName = token.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.GivenName).Value ?? string.Empty;
                var roles = token.Claims.Where(claim => claim.Type == ClaimTypes.Role).ToList();
                if (roles.Exists(claim => claim.Value == "Premium")) Constants.currentUser.Premium = true;
                else Constants.currentUser.Premium = false;
                await Settings.LoadSettings();
                new MainWindow().Show();
                this.Close();
            }
            else
            {
                // jak jest błąd to to :::
                ErrorMsgBlock.Text = msg["error"].ToString();
            }
        }

        private void RegisterHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (!RegistrationWindow.isOpened)
            {
                new RegistrationWindow().Show();
                RegistrationWindow.isOpened = true;
            }
        }

        private async void ForgotPassHyperlink_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailField.Text.Trim().TrimEnd();
            if (email == "")
            {
                ErrorMsgBlock.Text = "No login input";
                return;
            }
            else if (EmailChecker.IsValid(email))
            {
                ErrorMsgBlock.Text = "Wrong email format";
                return;
            }
            else
            {
                ErrorMsgBlock.Text = string.Empty;
            }
            var response = await HttpClientManager.ResetpassUserAsync(email);

            // zwraca pustego jsona
            // var msg = JObject.Parse(await response.Content.ReadAsStringAsync());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // jak jest dobrze to to :::
                ErrorMsgBlock.Foreground = Brushes.Green;
                ErrorMsgBlock.Text = "Check your email";
            }
            else
            {
                // jak jest błąd to to :::
                ErrorMsgBlock.Foreground = Brushes.Red;
                ErrorMsgBlock.Text = "Cannot send reset password email!";
            }
        }

        private void FaceLoginButton_Click(object sender, RoutedEventArgs e)
        {
            new FaceLoginWindow().Show();
            this.Close();
        }
    }
}