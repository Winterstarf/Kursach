using MainApp.assets.models;
using MainApp.classes;
using MainApp.pages;
using MainApp.windows.main;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Diagnostics;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ClientsPage clientsPage;
        public ServicesPage servicesPage;
        public OrdersPage ordersPage;

        public int CurrentUserId { get; set; }

        public MainWindow()
        {
            InitializeComponent();

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
            WindowController.ExitApp();
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
                WindowController.ShowAuthWindow();
            }
        }

        public void UpdateUserInfo(int userId, string fioShort)
        {
            var db_cont = new HelixDBEntities();
            var staff_member = db_cont.staff.FirstOrDefault(sr => sr.id == userId);

            CurrentUserId = userId;
            int id_role = staff_member.id_role; // 11 - all perms, 1 and 3 - limited
            App.IsLimitedPerms = id_role != 11;

            string rolename = db_cont.staff_roles.FirstOrDefault(srn => srn.id == id_role)?.role_name;
            CurrentDoctor_tb.Text = $"{fioShort}\n{rolename}";
        }

        public void ReloadPages()
        {
            clientsPage = new ClientsPage();
            servicesPage = new ServicesPage();
            ordersPage = new OrdersPage();
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

        private void PDFexport_btn_Click(object sender, RoutedEventArgs e)
        {
            var ew = new ExportWindow() {export_type = "pdf", export_page_name = $"{CurrentPage_tb.Text}"};
            ew.ShowDialog();
        }

        private void XLSXexport_btn_Click(object sender, RoutedEventArgs e)
        {
            var ew = new ExportWindow() {export_type = "xlsx", export_page_name = $"{CurrentPage_tb.Text}"};
            ew.ShowDialog();
        }

        private void Secret_btn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
