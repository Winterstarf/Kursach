using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void NumTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SqlConnection con = new SqlConnection(@"Data Source=201-04\SQLEXPRESS;Initial Catalog=Neva;Integrated Security=SSPI");
                con.Open();

                SqlCommand cmd = new SqlCommand("select * from UserData where Username='" + username + "' and Pass='" + password + "'", con);
                cmd.CommandType = CommandType.Text;

                object result = cmd.ExecuteScalar();

                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);

                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }

                con.Close();
            }
        }
    }
}