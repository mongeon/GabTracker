using Gadgeteer;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using System.IO;
using Microsoft.SPOT.IO;
using System;
using GabTracker;
using Microsoft.SPOT.Hardware;
using GHI.Usb.Host;
using System.Threading;
using System.Text;
namespace GabTracker
{
    public partial class Program
    {
        Gadgeteer.Timer _timer;
        double _temperature = -10000;
        double _humidity = -10000;
        private StorageDevice _storage;
        private bool _SDActivated = true;
        private static string _dataFilePath = @"\USB\GabTracker.csv";
        private TimeSpan _blinkInterval = new TimeSpan(0, 0, 0, 0, 100);

        System.Collections.Queue _positionQueue = new System.Collections.Queue();

        /// <summary>
        /// This method is run when the mainboard is powered up or reset.
        /// </summary>
        void ProgramStarted()
        {
            Debug.Print("Setting up...");

            _timer = new Gadgeteer.Timer(5000, Gadgeteer.Timer.BehaviorType.RunContinuously);
            _timer.Tick += _timer_Tick;

            btnSDToggle.TurnLedOff();
            btnSDToggle.Mode = Gadgeteer.Modules.GHIElectronics.Button.LedMode.ToggleWhenReleased;
            btnSDToggle.ButtonReleased += btnSDToggle_ButtonReleased;

            gps.InvalidPositionReceived += gps_InvalidPositionReceived;
            gps.PositionReceived += gps_PositionReceived;
            gps.DebugPrintEnabled = true;

            tempHumidity.DebugPrintEnabled = true;
            tempHumidity.MeasurementInterval = 2000;
            tempHumidity.MeasurementComplete += tempHumidity_MeasurementComplete;

            usbHost.MassStorageMounted += usbHost_MassStorageMounted;
            usbHost.MassStorageUnmounted += usbHost_MassStorageUnmounted;
            usbHost.DebugPrintEnabled = true;
            if (usbHost.IsMassStorageMounted)
            {
                _storage = usbHost.MassStorageDevice;
            }
            SetSDActivated(true);

            Debug.Print("Program Starting...");

            gps.Enabled = true;
            _timer.Start();
            tempHumidity.StartTakingMeasurements();
            Debug.Print("Program Started");
        }

        void usbHost_MassStorageUnmounted(USBHost sender, EventArgs e)
        {
            _storage = null;
        }

        void usbHost_MassStorageMounted(USBHost sender, StorageDevice device)
        {
            _storage = device;
        }

        void btnSDToggle_ButtonReleased(Gadgeteer.Modules.GHIElectronics.Button sender, Gadgeteer.Modules.GHIElectronics.Button.ButtonState state)
        {
            SetSDActivated(!btnSDToggle.IsLedOn);
        }

        private void SetSDActivated(bool value)
        {
            _SDActivated = value;
            if (_SDActivated)
            {
                ledSDActivated.TurnColor(Color.Orange);
            }
            else if (!_SDActivated)
            {
                ledSDActivated.TurnColor(Color.Red);
            }
        }

        void tempHumidity_MeasurementComplete(TempHumidity sender, TempHumidity.MeasurementCompleteEventArgs e)
        {
            PulseDebugLED();
            _temperature = e.Temperature;
            _humidity = e.RelativeHumidity;
        }

        void _timer_Tick(Gadgeteer.Timer timer)
        {
            PulseDebugLED();
            if (gps.Enabled && gps.LastPosition != null)
            {
                Debug.Print("Lat :" + gps.LastPosition.Latitude
                    + " - Long :" + gps.LastPosition.Longitude
                    + " - Speed : " + gps.LastPosition.SpeedKnots
                    + " - Age:" + gps.LastPosition.FixTimeUtc.ToString()
                    + " - Course" + gps.LastPosition.CourseDegrees);
            }
            if (_temperature > -1000 && _humidity > -1000)
            {
                Debug.Print("Temp :" + _temperature + " - Humidity : " + _humidity);
            }
            StoreInfo();
        }

        void gps_PositionReceived(GPS sender, GPS.Position e)
        {
            PulseDebugLED();
            Debug.Print("Position Received - " + e.FixTimeUtc);
            Utility.SetLocalTime(e.FixTimeUtc);
            _positionQueue.Enqueue(GetDataString(e, _temperature, _humidity));
                        ledSDActivated.TurnColor(Color.Green);
        }

        void gps_InvalidPositionReceived(GPS sender, EventArgs e)
        {
            Debug.Print("Invalid Position Received");
        }
        void StoreInfo()
        {
            if (_storage != null && _SDActivated)
            {
                if (_positionQueue.Count > 0)
                {
                    ledSDActivated.BlinkRepeatedly(Color.Green, _blinkInterval, Color.Blue, _blinkInterval);
                    Debug.Print("Storing");
                    try
                    {
                        string fullRecord = String.Empty;// GetDataString(position, temperature, relativeHumidity);

                        var files = _storage.ListFiles(_storage.RootDirectory);
                        while (_positionQueue.Count > 0)
                        {
                            fullRecord += _positionQueue.Dequeue().ToString() + "\r\n";
                        }

                        using (var fs = new FileStream(_dataFilePath, FileMode.Append))
                        {
                            byte[] data = Encoding.UTF8.GetBytes(fullRecord);
                            fs.Write(data, 0, data.Length);
                            fs.Flush();
                            fs.Close();
                        }

                        _storage.Volume.FlushAll();

                        Debug.Print("Local time: " + DateTime.Now + " ---- " + DateTime.UtcNow);
                        Debug.Print(fullRecord);

                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                    }
                }
                ledSDActivated.TurnColor(Color.Orange);
            }
            else
            {
                SetSDActivated(false);
            }

        }

        private static string GetDataString(GPS.Position position, double temperature, double relativeHumidity)
        {
            string recordPosition = DateTime.Now.ToString() + ";";
            if (position == null)
            {
                recordPosition += ";;;;";
            }
            else
            {
                recordPosition += position.Latitude + ";"
                    + position.Longitude + ";"
                    + position.SpeedKnots + ";"
                    + position.CourseDegrees + ";"
                    + position.FixTimeUtc;
            }
            string tempHumidityRecord = temperature.Round() + ";"
                + relativeHumidity.Round();

            string fullRecord = recordPosition + ";"
                + tempHumidityRecord;
            return fullRecord;
        }
    }
}
