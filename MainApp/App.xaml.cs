using System.Globalization;
using System;
using System.Windows;
using System.Windows.Data;
using MainApp.classes;
using System.Windows.Controls;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool IsAdmin { get; set; } = false;
        public static bool IsDoctor { get; set; } = false;
        public static bool IsLaborant { get; set; } = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            WindowController.ShowAuthWindow();
        }
    }
}
