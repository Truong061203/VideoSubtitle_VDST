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

            //var configPath = GlobalVariable.Instance.Path_Save_ComputerId;
            //SettingApp.Instance.LoadFileC(configPath);


            //var token = TokenRespositories.Instance.LoadToken();

            //// Thêm thời gian đệm an toàn (Buffer) là 5 phút
            //var bufferTime = TimeSpan.FromMinutes(5);
            //if (token != null && !string.IsNullOrEmpty(token.access_token) && (token.Expried - bufferTime) > DateTime.Now)
            //{

            //    var configWindow = new NewProjectWindow();
            //    this.MainWindow = configWindow;
            //    configWindow.Show();
            //}
            //else
            //{
            //    TokenRespositories.Instance.DeleteToken();
            //    var loginWindow = new LoginWindow();
            //    this.MainWindow = loginWindow;
            //    loginWindow.Show();
            //}

        }

    }

}
