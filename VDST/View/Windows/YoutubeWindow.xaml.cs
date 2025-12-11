using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VDST.ViewModel;

namespace VDST.View.Windows
{
    /// <summary>
    /// Interaction logic for YoutubeWindow.xaml
    /// </summary>
    public partial class YoutubeWindow : Window
    {
        public YoutubeWindow()
        {
            InitializeComponent();
            this.Closing += YoutubeWindow_Closing;
        }

        private void YoutubeWindow_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is YoutubeViewModel vm)
            {
                vm.OnWindowClosing();
            }
        }
    }
}
