using System.Globalization;
using System;
using System.Windows;
using System.Windows.Data;
using MainApp.classes;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static int CurrentUserId { get; set; }
        public static bool IsAdmin { get; set; } = false;
        public static bool IsDoctor { get; set; } = false;
        public static bool IsLaborant { get; set; } = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            WindowController.ShowAuthWindow();
        }

        internal static async Task ShowPopup(string message, Popup popup, TextBlock popup_text)
        {
            popup_text.Text = message;
            popup.IsOpen = true;
            await Task.Delay(5000);
            popup.IsOpen = false;
        }
    }
}
