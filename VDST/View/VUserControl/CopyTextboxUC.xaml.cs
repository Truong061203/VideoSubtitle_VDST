using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VDST.View.VUserControl
{
    /// <summary>
    /// Interaction logic for CopyTextboxUC.xaml
    /// </summary>
    public partial class CopyTextboxUC : UserControl
    {
        public CopyTextboxUC()
        {
            InitializeComponent();
        }



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CopyTextboxUC), new PropertyMetadata(string.Empty));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                Clipboard.SetText(Text);
                TextBlock content = new TextBlock()
                {
                    Text = "Đã vào chép",
                };
                ToolTip tooltip = new ToolTip
                {
                    Content = content,
                    PlacementTarget = BtnCopy,
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                    StaysOpen = false
                };
                BtnCopy.ToolTip = tooltip;
                tooltip.IsOpen = true;
                tooltip.IsOpen = false;
                BtnCopy.ToolTip = null;
            }
        }
    }
}
