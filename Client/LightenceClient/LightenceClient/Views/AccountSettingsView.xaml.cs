using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Image = System.Drawing.Image;

namespace LightenceClient.Views
{
    /// <summary>
    /// Logika interakcji dla klasy AccountSettings.xaml
    /// </summary>
    public partial class AccountSettings : UserControl
    {       
        VideoCapture capture;
        private Mat frame;
        private readonly CascadeClassifier cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
        private bool _isCaptureStarted;
        public byte[] savedFrame;

        private WriteableBitmap writeableBitmap;

        public AccountSettings()
        {
            InitializeComponent();
            _isCaptureStarted = false;
            tempLabel.Content = string.Empty;
            
            // To przenieść jak będzie czytanie czy zmieniono tab bo teraz wolno działa
            
        }
        
        public void SaveBitmapAsBytes(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Jpeg);
                savedFrame = stream.ToArray();
            }
        }
        
        private void ProcessFrame(object sender, EventArgs e)
        {
            if (capture != null && capture.Ptr != IntPtr.Zero && _isCaptureStarted)
            {
                capture.Retrieve(frame, 0);
                Dispatcher.Invoke(new Action(() => {
                    Detect();
                }));
            }
        }
        private void Detect()
        {
            var bitmap = frame.ToBitmap();
            Image<Gray, byte> grayImage = frame.ToImage<Gray, byte>();
            System.Drawing.Rectangle[] rectangles = cascadeClassifier.DetectMultiScale(grayImage, 1.1, 0);
            if (rectangles.Length > 0)
            {
                System.Drawing.Rectangle rectangle = rectangles[0];
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // Sprawdzamy czy wykryta twarz jest wystarczająco duża
                    if (rectangle.Height > 340 & rectangle.Width > 340)
                    {
                        using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 4))
                        {
                            graphics.DrawRectangle(pen, rectangle);
                        }
                    }
                    else
                    {
                        using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 4))
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
            writeableBitmap.Freeze();
            facePicture.Source = writeableBitmap;
        }

        private void TabAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(TabAccount.SelectedIndex != 1)
            {
                if(capture != null) capture.Stop();
            }
        }

        private void StartCameraCommand(object sender, RoutedEventArgs e)
        {
            if (capture != null)
            {
                try
                {
                    if (_isCaptureStarted == true)
                    {
                        capture.Stop();
                        _isCaptureStarted = false;
                    }
                    else
                    {
                        capture.ImageGrabbed += ProcessFrame;
                        frame = new Mat();
                        capture.Start();
                        _isCaptureStarted = true;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                capture = new VideoCapture(0, VideoCapture.API.DShow);
                capture.ImageGrabbed += ProcessFrame;
                frame = new Mat();
                capture.Start();
                _isCaptureStarted = true;
            }
        }
        
        private async void CreateUpdateBiometricProfileCommand(object sender, RoutedEventArgs e)
        {
            tempLabel.Content = string.Empty;
            MemoryStream ms = new MemoryStream();
            if (frame != null)
            {
                var bitmap = frame.ToBitmap();
                bitmap.Save(ms, ImageFormat.Jpeg);
                var response = await HttpClientManager.AddVisionProfileAsync(Constants.currentUser.Email, ms.ToArray());
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //capture.Dispose();
                    //facePicture.Source = null;
                    tempLabel.Foreground = System.Windows.Media.Brushes.Green;
                    tempLabel.Content = "Done!";
                    // wyświetlić sukces
                }
                else
                {
                    tempLabel.Foreground = System.Windows.Media.Brushes.Red;
                    tempLabel.Content = "Camera error";
                    // wyświetlić error i umożliwić ponowienie próby
                }
            }
        }

        private async void DeleteBiometricProfileCommand(object sender, RoutedEventArgs e)
        {
            tempLabel.Content = string.Empty;
            var response = await HttpClientManager.RemoveVisionProfileAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                //capture.Dispose();
                //facePicture.Source = null;
                tempLabel.Foreground = System.Windows.Media.Brushes.Green;
                tempLabel.Content = "Done!";
                // wyświetlić sukces
            }
            else
            {
                tempLabel.Foreground = System.Windows.Media.Brushes.Red;
                tempLabel.Content = "Server error!";
                // wyświetlić error i umożliwić ponowienie próby
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if(capture != null)
            {
                capture.Stop();
                capture.Dispose();
            }
        }
    }
}
