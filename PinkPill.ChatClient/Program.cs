using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace PinkPill.ChatClient
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
