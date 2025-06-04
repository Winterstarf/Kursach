using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MainApp;
using MainApp.windows.main;

namespace MainApp.classes
{
    public static class WindowController
    {
        private static AuthWindow authWindow;
        private static MainWindow mainWindow;

        public static void ShowAuthWindow()
        {
            if (authWindow == null)
                authWindow = new AuthWindow();

            authWindow.ClearInputs();
            authWindow.RestoreEyeState();
            RemoveFocus(authWindow);

            authWindow.Show();
            authWindow.Focus();

            mainWindow?.Hide();
        }

        public static void ShowMainWindow(int userId, string fioShort)
        {
            if (mainWindow == null)
                mainWindow = new MainWindow();

            mainWindow.Opacity = 0;
            mainWindow.UpdateUserInfo(userId, fioShort);
            mainWindow.ReloadPages();

            mainWindow.Clients_btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            mainWindow.Show();
            mainWindow.Focus();

            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            mainWindow.BeginAnimation(Window.OpacityProperty, fadeIn);
            RemoveFocus(mainWindow);

            authWindow?.Hide();
        }

        public static void ExitApp()
        {
            Application.Current.Shutdown();
        }

        public static void RemoveFocus(Window w)
        {
            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(w, null);
        }
    }
}
