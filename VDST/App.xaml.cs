using Serilog;
using Serilog.Core;
using System.Configuration;
using System.Data;
using System.Windows;
using VDST.Utils;

namespace VDST
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Information()
                  .WriteTo.Debug()
                  //.WriteTo.File("logs/applog-.txt", rollingInterval: RollingInterval.Day) // Ghi ra file, mỗi ngày 1 file mới
                  .CreateLogger();
            Log.Information("Application Starting Up.........................................\n");

            //--------------------------------------------------------------

            var configPath = GlobalVariable.Instance.Path_Save_ComputerId;

            SettingApp.Instance.LoadFileC(configPath);
        }


     
      
    }

}
