using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using System.Net;

namespace LightenceClient
{
    /// <summary>
    /// Logika interakcji dla klasy RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        static public bool isOpened = false;
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMsgBlock.Text = string.Empty;
            string Email = RegisterEmailField.Text.Trim().TrimEnd();
            string FirstName = RegisterFirstNameField.Text.Trim().TrimEnd();
            string LastName = RegisterLastNameField.Text.Trim().TrimEnd();
            string Password = PasswordField.Password;
            string ConfirmedPassword = ConfirmPasswordField.Password;

            if (EmailChecker.IsValid(Email) == false)
            {
                ErrorMsgBlock.Text = "Wrong e-mail format";
                return;
            }

            if (Email == "")
            {
                ErrorMsgBlock.Text = "No e-mail input";
                return;
            }
            if (FirstName == "")
            {
                ErrorMsgBlock.Text = "No first name input";
                return;
            }
            if (LastName == "")
            {
                ErrorMsgBlock.Text = "No last name input";
                return;
            }
            if (Password == "")
            {
                ErrorMsgBlock.Text = "No password input";
                return;
            }
            if (ConfirmedPassword == "")
            {
                ErrorMsgBlock.Text = "No confirmed password input";
                return;
            }
            else if (ConfirmedPassword != Password)
            {
                ErrorMsgBlock.Text = "Passwords aren't equal";
                return;
            }

            var response = await HttpClientManager.RegisterUserAsync(Email, Password, FirstName, LastName);

            var msg = JObject.Parse(await response.Content.ReadAsStringAsync());

            if (response.StatusCode == HttpStatusCode.Created)
            {
                // jak jest dobrze to to :::
                var _ = MessageBox.Show("Confirmation message was send. Please check your your e-mail.", "Verify e-mail", MessageBoxButton.OK, MessageBoxImage.Information);
                isOpened = false;
                Close();
            }
            else
            {
                // jak jest błąd to to :::
                ErrorMsgBlock.Text = msg["error"].ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isOpened = false;
            Close();
        }
    }
}