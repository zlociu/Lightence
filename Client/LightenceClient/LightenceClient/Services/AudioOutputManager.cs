using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace LightenceClient.Services
{
    /// <summary>
    /// Manage receiving data and playing sound on output device
    /// </summary>
    class AudioOutputManager: IDisposable
    {
        public static int AudioOutputDeviceNumber = 0; //speaker/headphones

        private WaveOut player;
        private BufferedWaveProvider buffer;
        // Pomiędzy 0.0f a 1.0f
        public float Volume { get; set; }

        public AudioOutputManager()
        {
            buffer = new BufferedWaveProvider(new WaveFormat(8000, 8, 1));
            Volume = 0.8f;
            player = new WaveOut()
            {
                DeviceNumber = AudioOutputDeviceNumber,
                Volume = this.Volume, 
                DesiredLatency = 700,
                NumberOfBuffers = 3,
            };
            player.Init(buffer);
        }

        public void AddData(byte[] data)
        {
            buffer.AddSamples(data, 0, data.Length);
        }

        public void Play()
        {
            player.Play();
        }

        public void Stop()
        {
            player.Stop();
            buffer.ClearBuffer();
        }

        public void Dispose()
        {
            player.Dispose();
        }

        #region Static functions
        public static List<string> GetAudioOutputDevicesInfo()
        {
            var devices = new List<string>();
            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                devices.Add($"{caps.ProductName}");
            }
            return devices;
        }

        public static int GetAudioOutputDeviceNumberByName(string name)
        {
            int number = 0;
            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                if (caps.ProductName == name)
                {
                    number = n; break;
                }
            }
            // TODO what if there is no such name
            return number;
        }

        public static void SetAudioOutputDevice(int deviceNumber)
        {
            AudioOutputDeviceNumber = deviceNumber;
        }

        #endregion
    }
}
