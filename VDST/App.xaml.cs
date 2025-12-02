using Serilog;
using Serilog.Core;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using VDST.Repositories;
using VDST.Utils;
using VDST.View;
using VDST.View.Windows;

namespace VDST
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
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


            var token = TokenRespositories.Instance.LoadToken();

            if (token != null && !string.IsNullOrEmpty(token.access_token) && token.Expried > DateTime.Now)
            {
             
                var configWindow = new NewProjectConfiguration();
                this.MainWindow = configWindow; // Đánh dấu đây là cửa sổ chính
                configWindow.Show();
            }
            else
            {
                if (token != null) TokenRespositories.Instance.ClearCrendential();

                var loginWindow = new LoginWindow();
                this.MainWindow = loginWindow; // Đánh dấu đây là cửa sổ chính
                loginWindow.Show();
            }

        }
     




    }

}
