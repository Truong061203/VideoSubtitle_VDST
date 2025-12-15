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

        private void TextBoxUC_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                if (DataContext is YoutubeViewModel vm)
                {
                    vm.FilterSubtitles();   // đọc SearchSubText mới nhất
                    Keyboard.ClearFocus();  // cho UX đẹp
                }
            }
        }

        private void TextBoxUC_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;


                if (this.DataContext is YoutubeViewModel vm)
                {
                    vm.OnCheck(null);

                    Keyboard.ClearFocus();
                }
            }
        }
    }
}
