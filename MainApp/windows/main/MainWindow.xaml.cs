using MainApp.pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ClientsPage clientsPage;
        readonly ServicesPage servicesPage;
        readonly OrdersPage ordersPage;
        private AuthWindow authWindow;

        public string CurrentUserId { get; set; }
        public string CurrentUserName { get; set; }
        public string CurrentUserRole { get; set; }
        public string CurrentUserFullName { get; set; }

        public MainWindow(AuthWindow aw)
        {
            InitializeComponent();

            authWindow = aw;

            clientsPage = new ClientsPage();
            servicesPage = new ServicesPage();
            ordersPage = new OrdersPage();

            Clients_btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void Clients_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage_tb.Text = "Клиенты";
            DG_frm.NavigationService.Navigate(clientsPage);
        }

        private void Services_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage_tb.Text = "Услуги";
            DG_frm.NavigationService.Navigate(servicesPage);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //i dont know why this works, dont add anything here
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void Orders_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage_tb.Text = "Архив заказов";
            DG_frm.NavigationService.Navigate(ordersPage);
        }

        private void ChangeUser_btn_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Вы уверены, что хотите сменить пользователя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                authWindow.UsernameTB.Text = string.Empty;
                authWindow.PassTB.Text = string.Empty;
                authWindow.PassPB.Password = string.Empty;
                authWindow.RestoreEyeState();
                authWindow.Focus();
                authWindow.Show();

                this.Hide();
            }
        }

        public void UpdateUserInfo(string userId, string userName, string userRole, string fullName, string fioShort)
        {
            CurrentUserId = userId;
            CurrentUserName = userName;
            CurrentUserRole = userRole;
            CurrentUserFullName = fullName;

            CurrentDoctor_tb.Text = $"{fioShort}\n{userRole}";
        }

        public void SetAuthWindow(AuthWindow aw)
        {
            authWindow = aw;
        }

        private CustomPopupPlacement[] OnCustomPopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            // offsets on x and y axises
            double offsetX = targetSize.Width + 3;
            double offsetY = targetSize.Height - popupSize.Height + 5;

            return new CustomPopupPlacement[]
            {
                new CustomPopupPlacement(new Point(offsetX, offsetY), PopupPrimaryAxis.Horizontal)
            };
        }
    }
}
