using MainApp.assets.models;
using MainApp.pages;
using System.Windows;
using System.Windows.Controls;

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

        public string CurrentUserName { get; set; }
        public string CurrentUserRole { get; set; }
        public string CurrentUserFullName { get; set; }

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
            System.Environment.Exit(0);
        }

        private void Orders_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage_tb.Text = "Архив заказов";
            DG_frm.NavigationService.Navigate(ordersPage);
        }
    }
}
