using MainApp.classes;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private bool isPasswordVisible = false;

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = Convert.ToString(UsernameTB.Text);
            string password = PassPB.Password;

            try
            {
                using (SqlConnection con = new SqlConnection(@"Data Source=DESKTOP-QLMK9N;Initial Catalog=HelixDB;Integrated Security=SSPI"))
                {
                    con.Open(); // will throw if the server isn't running or unreachable

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
                        WindowController.RemoveFocus(this);

                        int userId = Convert.ToInt32(dataSet.Tables[0].Rows[0]["id"]);
                        string roleName = dataSet.Tables[0].Rows[0]["role_name"].ToString();

                        string lastName = dataSet.Tables[0].Rows[0]["last_name"].ToString();
                        string firstName = dataSet.Tables[0].Rows[0]["first_name"].ToString();
                        string middleName = dataSet.Tables[0].Rows[0]["middle_name"] == DBNull.Value
                            ? ""
                            : dataSet.Tables[0].Rows[0]["middle_name"].ToString();

                        string fioShort = $"{lastName} {firstName[0]}." + (!string.IsNullOrEmpty(middleName) ? $"{middleName[0]}." : "");

                        WindowController.ShowMainWindow(userId, fioShort);
                    }
                    else if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password))
                        MessageBox.Show("Логин и пароль не введены");
                    else if (string.IsNullOrWhiteSpace(username))
                        MessageBox.Show("Логин не введён");
                    else if (string.IsNullOrWhiteSpace(password))
                        MessageBox.Show("Пароль не введён");
                    else
                        MessageBox.Show("Аккаунт не найден");
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Убедитесь, что сервер MSSQL запущен.\n\n" + sqlEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка:\n" + ex.Message);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowController.ExitApp();
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

        private void PassTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            PassPB.Password = PassTB.Text;
        }

        /*
        private void Password_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // Явно снимаем фокус с текущего поля ввода
                Keyboard.ClearFocus();  // Снимет фокус с текстовых полей
                FocusManager.SetFocusedElement(this, null);  // Дополнительно снимаем фокус

                // Обрабатываем логин, как если бы кнопка была нажата
                LoginBtn_Click(sender, new RoutedEventArgs());
            }
        }*/

        private void Window_Closed(object sender, EventArgs e)
        {
            WindowController.ExitApp();
        }

        public void RestoreEyeState()
        {
            if (isPasswordVisible)
            {
                PassTB.Visibility = Visibility.Visible;
                PassPB.Visibility = Visibility.Collapsed;
                eye_btn_img.Source = new BitmapImage(new Uri("/assets/images/eye_closed.png", UriKind.Relative));
            }
            else
            {
                PassPB.Visibility = Visibility.Visible;
                PassTB.Visibility = Visibility.Collapsed;
                eye_btn_img.Source = new BitmapImage(new Uri("/assets/images/eye_open.png", UriKind.Relative));
            }
        }

        public void ClearInputs()
        {
            UsernameTB.Text = string.Empty;
            PassPB.Password = string.Empty;
            PassTB.Text = string.Empty;
        }
    }
}
