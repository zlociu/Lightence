using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Windows;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Drawing.Imaging;
using System.Windows.Media;
using Pen = System.Drawing.Pen;
using Size = System.Drawing.Size;

namespace LightenceClient
{
    /// <summary>
    /// Logika interakcji dla klasy FaceLoginWindow.xaml
    /// </summary>
    /// 
    public partial class FaceLoginWindow : Window
    {
        VideoCapture capture;
        private Mat frame;
        private readonly CascadeClassifier cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
        public bool isSent = false;
        private bool _isCaptureStarted;

        private WriteableBitmap writeableBitmap;

        internal static class NativeMethods
        {
            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);
        }

        public FaceLoginWindow()
        {
            InitializeComponent();
            _isCaptureStarted = true;

            capture = new VideoCapture(0, VideoCapture.API.DShow);
            capture.ImageGrabbed += ProcessFrame;
            frame = new Mat();
            capture.Start();
        }

        private void BackLogin_Click(object sender, RoutedEventArgs e)
        {
            capture.Stop();
            capture.Dispose();
            new LoginWindow().Show();
            this.Close();
        }
        public static byte[] ImageToByte(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }
        private void ProcessFrame(object sender, EventArgs e)
        {
            if (capture != null && capture.Ptr != IntPtr.Zero)
            {
                capture.Retrieve(frame);
                Dispatcher.Invoke(new Action(() => {
                   Detect();
                }));
            }
        }

        private void Detect()
        {
            var bitmap = frame.ToBitmap();
            
            Image<Gray, byte> grayImage = frame.ToImage<Gray, byte>();
            var rectangles = cascadeClassifier.DetectMultiScale(grayImage, 1.1, 0);
            if (rectangles.Length > 0)
            {
                var rectangle = rectangles[0];
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // Sprawdzamy czy wykryta twarz jest wystarczająco duża
                    if (rectangle.Height > 310 && rectangle.Width > 310)
                    {
                        using (Pen pen = new Pen(System.Drawing.Color.Blue, 4))
                        {
                            graphics.DrawRectangle(pen, rectangle);
                        }
                    }
                    else
                    {
                        using (Pen pen = new Pen(System.Drawing.Color.Red, 4))
                        {
                            graphics.DrawRectangle(pen, rectangle);
                        }
                    }
                }
            }
            
            writeableBitmap = new WriteableBitmap(System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
               bitmap.GetHbitmap(),
               IntPtr.Zero,
               System.Windows.Int32Rect.Empty,
               BitmapSizeOptions.FromWidthAndHeight(500, 300)));
            
            //var pic = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            //   bitmap.GetHbitmap(),
            //   IntPtr.Zero,
            //  System.Windows.Int32Rect.Empty,
            //   BitmapSizeOptions.FromWidthAndHeight(500, 300));
            writeableBitmap.Freeze();
            loginPicture.Source = writeableBitmap;
        }

        private void StartCamera_Click(object sender, RoutedEventArgs e)
        {
            if (capture != null)
            {
                try
                {
                    if (_isCaptureStarted == true)
                    {
                        capture.Stop();
                        _isCaptureStarted = false;
                        StartCamera.Background = new SolidColorBrush(System.Windows.Media.Colors.Red);
                    }
                    else
                    {
                        capture.Start();
                        _isCaptureStarted = true;
                        StartCamera.Background = new SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            tempLabel.Content = string.Empty;
            string email = EmailTextBox.Text.Trim().TrimEnd();
            if (string.IsNullOrEmpty(email) == false)
            {
                var bitmap = frame.ToBitmap();
                if (bitmap != null)
                {
                    var response = await HttpClientManager.LoginVisionUserAsync(email, ImageToByte(bitmap));

                    var msg = JObject.Parse(await response.Content.ReadAsStringAsync());
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        capture.Stop();
                        capture.Dispose();

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
                        tempLabel.Foreground = System.Windows.Media.Brushes.Red;
                        tempLabel.Content = "Login failed";
                        //jakieś komunikaty gdzieś, tylko trzeba errorbox dodać
                    }
                }
                else
                {
                    tempLabel.Foreground = System.Windows.Media.Brushes.Red;
                    tempLabel.Content = "Camera error";
                    //jakieś komunikaty gdzieś, tylko trzeba errorbox dodać
                }
            }
            else
            {
                tempLabel.Foreground = System.Windows.Media.Brushes.Red;
                tempLabel.Content = "Empty e-mail";
                //jakieś komunikaty gdzieś, tylko trzeba errorbox dodać
            }
        }
    }

}