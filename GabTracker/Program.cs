using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;

namespace GabTracker
{
    public partial class Program
    {
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
            gps.InvalidPositionReceived += gps_InvalidPositionReceived;
            gps.NmeaSentenceReceived += gps_NmeaSentenceReceived;
            gps.PositionReceived += gps_PositionReceived;
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        void gps_PositionReceived(GPS sender, GPS.Position e)
        {
            Debug.Print("Position Received");
        }

        void gps_NmeaSentenceReceived(GPS sender, string e)
        {
            Debug.Print("NMEA sentence: " + e);
        }

        void gps_InvalidPositionReceived(GPS sender, EventArgs e)
        {
            Debug.Print("Invalid Position Received");
        }
    }
}
