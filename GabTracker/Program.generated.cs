//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.34014
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GabTracker {
    using Gadgeteer;
    using GTM = Gadgeteer.Modules;
    
    
    public partial class Program : Gadgeteer.Program {
        
        /// <summary>The GPS module using socket 11 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.GPS gps;
        
        /// <summary>The USB Client DP module using socket 1 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.USBClientDP usbClientDP;
        
        /// <summary>The Temp&Humidity module using socket 14 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.TempHumidity tempHumidity;
        
        /// <summary>The SD Card module using socket 5 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.SDCard sdCard;
        
        /// <summary>The Button module using socket 10 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.Button btnSDToggle;
        
        /// <summary>This property provides access to the Mainboard API. This is normally not necessary for an end user program.</summary>
        protected new static GHIElectronics.Gadgeteer.FEZSpider Mainboard {
            get {
                return ((GHIElectronics.Gadgeteer.FEZSpider)(Gadgeteer.Program.Mainboard));
            }
            set {
                Gadgeteer.Program.Mainboard = value;
            }
        }
        
        /// <summary>This method runs automatically when the device is powered, and calls ProgramStarted.</summary>
        public static void Main() {
            // Important to initialize the Mainboard first
            Program.Mainboard = new GHIElectronics.Gadgeteer.FEZSpider();
            Program p = new Program();
            p.InitializeModules();
            p.ProgramStarted();
            // Starts Dispatcher
            p.Run();
        }
        
        private void InitializeModules() {
            this.gps = new GTM.GHIElectronics.GPS(11);
            this.usbClientDP = new GTM.GHIElectronics.USBClientDP(1);
            this.tempHumidity = new GTM.GHIElectronics.TempHumidity(14);
            this.sdCard = new GTM.GHIElectronics.SDCard(5);
            this.btnSDToggle = new GTM.GHIElectronics.Button(10);
        }
    }
}
