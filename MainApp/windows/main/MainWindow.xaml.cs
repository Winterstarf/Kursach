using MainApp.assets.models;
using MainApp.classes;
using MainApp.pages;
using MainApp.windows.main;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System;
using System.Collections.Generic;

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
            int id_role = staff_member.id_role; // 11 - all perms, 1 - doctor (add clients, orders | edit orders)
            App.IsDoctor = id_role == 1; // 8 - laborant (edit orders)
            App.IsLaborant = id_role == 8;
            App.IsAdmin = id_role == 11;

            if (App.IsDoctor) { App.IsLaborant = false; }
            else if (App.IsLaborant) { App.IsDoctor = false; }
            else if (App.IsAdmin) { App.IsDoctor = false; App.IsLaborant = false; }

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

        private async void HandleExport(string exportType)
        {
            string pageName = CurrentPage_tb.Text.Trim();
            List<int> selectedIds = null;

            if (pageName == "Клиенты") selectedIds = clientsPage.DG_Clients.SelectedItems.Cast<clients>().Select(c => c.id).ToList();
            else if (pageName == "Услуги") selectedIds = servicesPage.DG_Services.SelectedItems.Cast<medical_services>().Select(s => s.id).ToList();
            else if (pageName == "Архив заказов") selectedIds = ordersPage.DG_Orders.SelectedItems.Cast<OrderDisplay>().Select(o => o.OrderId).Distinct().ToList();

            if (selectedIds != null && selectedIds.Any())
            {
                var res = MessageBox.Show("Экспортировать выделенные строки? \"Да\" — экспорт выбранных, \"Нет\" — открыть окно экспорта.", "Экспорт", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Yes)
                {
                    await Exporter.ExportSelectedByIdsAsync(selectedIds, exportType, pageName);
                    return;
                }
                else if (res == MessageBoxResult.No)
                {
                    var ew = new ExportWindow() { export_type = exportType, export_page_name = pageName };
                    ew.ShowDialog();
                    return;
                }
                else return;
            }
            else
            {
                // if no rows selected
                var ew = new ExportWindow() { export_type = exportType, export_page_name = pageName };
                ew.ShowDialog();
            }
        }

        private void PDFexport_btn_Click(object sender, RoutedEventArgs e)
        {
            HandleExport("pdf");
        }

        private void XLSXexport_btn_Click(object sender, RoutedEventArgs e)
        {
            HandleExport("xlsx");
        }

        private void Secret_btn_Click(object sender, RoutedEventArgs e)
        {
            string funny = "https://store.steampowered.com/app/420530/OneShot/";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = funny,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть смешнявку: " + ex.Message);
            }
        }
    }
}
