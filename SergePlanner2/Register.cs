using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SergePlanner2
{
    public partial class Register : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=SergeB;Initial Catalog=SergePlanner;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

        public Register()
        {
            InitializeComponent();
        }

        private void registerBtn_Click(object sender, EventArgs e)
        {
            if (usernameBox.Text.Trim() == "" || passwordBox.Text.Trim() == "" || comfirmPasswordBox.Text.Trim() == "")
            {
                MessageBox.Show("All fields are required!");
                return;
            }

            if (passwordBox.Text != comfirmPasswordBox.Text)
            {
                MessageBox.Show("Passwords do not match!");
                return;
            }

            if (!termsCkBox.Checked)
            {
                MessageBox.Show("You must agree to the Terms and Conditions!");
                return;
            }

            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                // check if user already exists
                string checkUser = "SELECT COUNT(*) FROM [User] WHERE email=@email";
                SqlCommand checkCmd = new SqlCommand(checkUser, conn);
                checkCmd.Parameters.AddWithValue("@email", usernameBox.Text.Trim());
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    MessageBox.Show("User already exists!");
                    return;
                }

                // insert new user
                string query = "INSERT INTO [User] (email, password) VALUES (@email, @password)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email", usernameBox.Text.Trim());
                cmd.Parameters.AddWithValue("@password", passwordBox.Text.Trim());
                cmd.ExecuteNonQuery();

                MessageBox.Show("Registration successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Go to login
                Login login = new Login();
                login.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during registration: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void termsCkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (termsCkBox.Checked)
                termsCkBox.Text = "✔ I Agree";
            else
                termsCkBox.Text = "I agree to Terms & Conditions";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //goto login
            Login login = new Login();
            login.Show();
            this.Hide();
        }
    }
}
