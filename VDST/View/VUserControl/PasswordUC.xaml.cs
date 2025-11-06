using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VDST.View.VUserControl
{
    /// <summary>
    /// Interaction logic for PasswordUC.xaml
    /// </summary>
    public partial class PasswordUC : UserControl
    {
        public PasswordUC()
        {
            InitializeComponent();
            UpdatePlaceholderVisibility();

        }
        private bool _isPasswordVisible = false;

        // Properties Binding Two Way
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(PasswordUC),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

        // hiển thị chữ mờ nhập pass cho textBlock
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(PasswordUC),
                new PropertyMetadata("Enter password..."));


        // Events password changed
        public event RoutedEventHandler PasswordChanged;

        /// <summary>
        /// button ẩn hiện mật khẩu
        /// </summary>
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // Show password as text
                textBox.Text = passwordBox.Password;
                textBox.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Hidden;
                iconPresenter.Content = FindResource("EyeIcon");
                textBox.Focus();
                textBox.CaretIndex = textBox.Text.Length;
            }
            else
            {
                // Hide password
                passwordBox.Password = textBox.Text;
                passwordBox.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Hidden;
                iconPresenter.Content = FindResource("EyeSlashIcon");
                passwordBox.Focus();
                var hwnd = ((HwndSource)PresentationSource.FromVisual(passwordBox)).Handle;
                PostMessage(hwnd, WM_KEYDOWN, (IntPtr)VK_END, IntPtr.Zero);
            }

            UpdatePlaceholderVisibility();
        }

        /// <summary>
        /// 
        /// </summary>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isPasswordVisible)
            {
                Password = passwordBox.Password;
                textBox.Text = passwordBox.Password;
                UpdatePlaceholderVisibility();
                PasswordChanged?.Invoke(this, e);
            }
        }
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_KEYDOWN = 0x0100;
        const int VK_END = 0x23;

        // Call after .Focus()


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                Password = textBox.Text;
                passwordBox.Password = textBox.Text;
                UpdatePlaceholderVisibility();
                PasswordChanged?.Invoke(this, new RoutedEventArgs());
            }
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PasswordUC)d;
            var newPassword = (string)e.NewValue;

            if (control.passwordBox.Password != newPassword)
            {
                control.passwordBox.Password = newPassword;
            }

            if (control.textBox.Text != newPassword)
            {
                control.textBox.Text = newPassword;
            }

            control.UpdatePlaceholderVisibility();
        }

        private void UpdatePlaceholderVisibility()
        {
            bool hasText = !string.IsNullOrEmpty(Password);
            placeholderText.Visibility = hasText ? Visibility.Hidden : Visibility.Visible;
        }
        public void Clear()
        {
            Password = string.Empty;
            passwordBox.Clear();
            textBox.Clear();
            UpdatePlaceholderVisibility();
        }
    }
}
