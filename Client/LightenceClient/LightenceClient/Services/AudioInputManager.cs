using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace LightenceClient.Services
{
    /// <summary>
    /// Manage recording from input device and sending data 
    /// </summary>
    class AudioInputManager : IDisposable
    {
        public static int AudioInputDeviceNumber = 0; //microphone

        //public readonly object _lock = new object();

        public event Action<byte[], int> SendBuffer;

        private WaveIn recorder;
        // Zmienna do zaznaczenia czy wysyłamy audio

        public AudioInputManager()
        {
            // numer uzyskany z getDevice dawać 0 domyślnie
            recorder = new WaveIn()
            {
                DeviceNumber = AudioInputDeviceNumber,
                WaveFormat = new WaveFormat(8000, 8, 1),
                NumberOfBuffers = 3,
                BufferMilliseconds = 50
            };
            recorder.DataAvailable += GotData;
        }

        private void GotData(object sender, WaveInEventArgs waveInEventArgs)
        {
            //double avg = waveInEventArgs.Buffer[0..waveInEventArgs.BytesRecorded].Select(x => (int)x).Average();
            //SendBuffer?.Invoke(waveInEventArgs.Buffer, waveInEventArgs.BytesRecorded);
            //if (Math.Abs(avg - 128.0) > 1)
            //{
                //string log = Constants.currentUser.Email + " received: " + avg.ToString();
                //Loggers.Audio_logger.Info(log);
                SendBuffer?.Invoke(waveInEventArgs.Buffer, waveInEventArgs.BytesRecorded);
            //}
        }

        public void Start()
        {
            recorder.StartRecording();
        }

        public void Stop()
        {
            recorder.StopRecording();
        }

        public void Dispose()
        {
            recorder.Dispose();
        }

        #region Static Functions

        public static List<string> GetAudioInputDevicesInfo()
        {
            var devices = new List<string>();
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var caps = WaveIn.GetCapabilities(n);
                devices.Add($"{caps.ProductName}");
            }
            return devices;
        }

        public static int GetAudioInputDeviceNumberByName(string name)
        {
            int number = 0;
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var caps = WaveIn.GetCapabilities(n);
                if (caps.ProductName == name)
                {
                    number = n; break;
                }
            }
            // TODO what if there is no such name
            return number;
        }

        public static void SetAudioInputDevice(int deviceNumber)
        {
            AudioInputDeviceNumber = deviceNumber;
        }
        #endregion

    }
}
