using System.Windows;

namespace MainApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            AuthWindow a = new AuthWindow();
            this.Owner = a;
        }
    }
}
