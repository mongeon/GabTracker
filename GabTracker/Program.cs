using Gadgeteer;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using System.IO;
using Microsoft.SPOT.IO;
using System;
using GabTracker;
using Microsoft.SPOT.Hardware;

namespace GabTracker
{
    public partial class Program
    {
        Timer _timer;
        double _temperature = -10000;
        double _humidity = -10000;
        private StorageDevice _storage;
        private bool _SDActivated = false;
        private static string _dataFilePath = @"\SD\AirQuality.TXT";

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/
            Debug.Print("Setting up...");
            SetSDActivated();
            _timer = new Timer(5000, Timer.BehaviorType.RunContinuously);
            _timer.Tick += _timer_Tick;


            btnSDToggle.TurnLedOff();
            btnSDToggle.Mode = Gadgeteer.Modules.GHIElectronics.Button.LedMode.ToggleWhenReleased;
            btnSDToggle.ButtonReleased += btnSDToggle_ButtonReleased;

            gps.InvalidPositionReceived += gps_InvalidPositionReceived;
            gps.NmeaSentenceReceived += gps_NmeaSentenceReceived;
            gps.PositionReceived += gps_PositionReceived;
            gps.DebugPrintEnabled = true;

            tempHumidity.DebugPrintEnabled = true;
            tempHumidity.MeasurementInterval = 2000;
            tempHumidity.MeasurementComplete += tempHumidity_MeasurementComplete;

            sdCard.Mounted += sdCard_Mounted;
            sdCard.Unmounted += sdCard_Unmounted;

            if (sdCard.IsCardMounted)
            {
                _storage = sdCard.StorageDevice;
            }
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            gps.Enabled = true;
            _timer.Start();
            tempHumidity.StartTakingMeasurements();
        }

        void btnSDToggle_ButtonReleased(Gadgeteer.Modules.GHIElectronics.Button sender, Gadgeteer.Modules.GHIElectronics.Button.ButtonState state)
        {
            _SDActivated = !btnSDToggle.IsLedOn;
            SetSDActivated();
        }

        private void SetSDActivated()
        {
            if (_SDActivated && ledSDActivated.GetCurrentColor() != Color.Green)
            {
                ledSDActivated.TurnColor(Color.Green);
            }
            else if (!_SDActivated && ledSDActivated.GetCurrentColor() != Color.Red)
            {
                ledSDActivated.TurnColor(Color.Red);
            }
        }

        void sdCard_Mounted(SDCard sender, StorageDevice device)
        {
            if (sdCard.IsCardMounted)
            {
                _storage = sdCard.StorageDevice;
            }
        }

        void sdCard_Unmounted(SDCard sender, EventArgs e)
        {
            _storage = null;

        }



        void tempHumidity_MeasurementComplete(TempHumidity sender, TempHumidity.MeasurementCompleteEventArgs e)
        {
            _temperature = e.Temperature;
            _humidity = e.RelativeHumidity;

        }

        void _timer_Tick(Timer timer)
        {
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

            StoreInfo(gps.LastPosition, _temperature, _humidity);
        }

        void gps_PositionReceived(GPS sender, GPS.Position e)
        {
            Debug.Print("Position Received - " + e.FixTimeUtc);
            Utility.SetLocalTime(e.FixTimeUtc);
        }

        void gps_NmeaSentenceReceived(GPS sender, string e)
        {
            //   Debug.Print("NMEA sentence: " + e);
        }

        void gps_InvalidPositionReceived(GPS sender, EventArgs e)
        {
            Debug.Print("Invalid Position Received");
        }
        void StoreInfo(GPS.Position position, double temperature, double relativeHumidity)
        {
            if (_storage != null && _SDActivated)
            {
                ledSDActivated.BlinkRepeatedly(Color.Green);
                Debug.Print("Storing");
                try
                {
                    string recordPosition;
                    if (position == null)
                    {
                        recordPosition = ";;;;";
                    }
                    else
                    {
                        recordPosition = position.Latitude + ";"
                            + position.Longitude + ";"
                            + position.SpeedKnots + ";"
                            + position.CourseDegrees + ";"
                            + position.FixTimeUtc;
                    }
                    string tempHumidityRecord = temperature.Round() + ";"
                        + relativeHumidity.Round();

                    string fullRecord = recordPosition + ";"
                        + tempHumidityRecord;

                    var sw = new StreamWriter(File.Open(_dataFilePath, FileMode.Append, FileAccess.Write));
                    sw.WriteLine(fullRecord);
                    sw.Flush();
                    sw.Close();
                    Debug.Print("Local time: " + DateTime.Now + " ---- " + DateTime.UtcNow);
                    Debug.Print(fullRecord);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }
            }
            else
            {
                if (_SDActivated)
                {
                    ledSDActivated.BlinkRepeatedly(Color.Green);
                }
            }

        }
    }
}
