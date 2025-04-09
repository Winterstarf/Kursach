using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public string Password { get; set; } = "";

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = Convert.ToString(UsernameTB.Text);
            string password = Convert.ToString(Password);

            SqlConnection con = new SqlConnection(@"Data Source=DESKTOP-QLMK9N;Initial Catalog=HelixDB;Integrated Security=SSPI");
            con.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT s.*, r.role_name 
                FROM staff s 
                JOIN staff_roles r ON s.id_role = r.id 
                WHERE s.staff_login = @username AND s.staff_pwd = @password", con);

            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            SqlDataAdapter adapter = new SqlDataAdapter { SelectCommand = cmd };
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);

            if (dataSet.Tables[0].Rows.Count > 0)
            {
                string roleName = dataSet.Tables[0].Rows[0]["role_name"].ToString();

                string lastName = dataSet.Tables[0].Rows[0]["last_name"].ToString();
                string firstName = dataSet.Tables[0].Rows[0]["first_name"].ToString();
                string middleName = dataSet.Tables[0].Rows[0]["middle_name"] == DBNull.Value
                    ? ""
                    : dataSet.Tables[0].Rows[0]["middle_name"].ToString();

                string fullName = $"{lastName} {firstName} {middleName}".Trim();

                MainWindow m = new MainWindow
                {
                    CurrentUserName = username,
                    CurrentUserRole = roleName,
                    CurrentUserFullName = fullName
                };

                string fioShort = $"{lastName} {firstName[0]}.";
                if (!string.IsNullOrEmpty(middleName)) fioShort += $"{middleName[0]}.";

                m.CurrentDoctor_tb.Text = $"{fioShort}\n{m.CurrentUserRole}";
                m.Show();
                this.Close();
            }
            else if ((username == "" || username == null) && (password == "" || password == null)) MessageBox.Show("Логин и пароль не введены");
            else if (username == "" || username == null) MessageBox.Show("Логин не введён");
            else if (password == "" || password == null) MessageBox.Show("Пароль не введён");
            else MessageBox.Show("Аккаунт не найден");

            con.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void PassPB_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = PassPB.Password;
        }

        private void eye_btn_Click(object sender, RoutedEventArgs e)
        {
            if (PassPB.Visibility == Visibility.Visible)
            {
                PassTB.Text = PassPB.Password;
                PassTB.Visibility = Visibility.Visible;
                PassPB.Visibility = Visibility.Collapsed;

                eye_btn_img.Source = new BitmapImage(new Uri("/assets/images/eye_closed.png", UriKind.Relative));
            }
            else
            {
                PassPB.Password = PassTB.Text;
                PassPB.Visibility = Visibility.Visible;
                PassTB.Visibility = Visibility.Collapsed;

                eye_btn_img.Source = new BitmapImage(new Uri("/assets/images/eye_open.png", UriKind.Relative));
            }
        }
    }
}
