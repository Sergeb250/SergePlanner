using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SergePlanner2
{
    public partial class Planning : Form
    {
        private string loggedUser; 
        SqlConnection conn = new SqlConnection(@"Data Source=SergeB;Initial Catalog=SergePlanner;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        private DataGridView dgv;

        public Planning(string email)
        {
            InitializeComponent();
            loggedUser = email;
            InitializeDataGridView();
        }

        // Initialize DataGridView inside plannTable panel
        private void InitializeDataGridView()
        {
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.CellClick += Dgv_CellClick;

            plannTable.Controls.Add(dgv);
        }

        //  Load all planning data on form load
        private void Planning_Load(object sender, EventArgs e)
        {
            LoadPlanningData();
        }

        //  Add new planning + task
        private void registerBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(taskNameBox.Text) || string.IsNullOrEmpty(statusCBox.Text))
                {
                    MessageBox.Show("Please fill all required fields.");
                    return;
                }

                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                // Step 1: Add new Task
                string insertTask = "INSERT INTO Task (taskName, status) VALUES (@taskName, @status)";
                SqlCommand cmdTask = new SqlCommand(insertTask, conn);
                cmdTask.Parameters.AddWithValue("@taskName", taskNameBox.Text.Trim());
                cmdTask.Parameters.AddWithValue("@status", statusCBox.Text.Trim());
                cmdTask.ExecuteNonQuery();

                // Step 2: Get new taskId
                string getTaskId = "SELECT taskId FROM Task WHERE taskName=@taskName";
                SqlCommand cmdId = new SqlCommand(getTaskId, conn);
                cmdId.Parameters.AddWithValue("@taskName", taskNameBox.Text.Trim());
                int taskId = Convert.ToInt32(cmdId.ExecuteScalar());

                // Step 3: Add new Planning linked to user
                string insertPlanning = "INSERT INTO Planning (taskId, email, startDate, endDate) VALUES (@taskId, @em, @start, @end)";
                SqlCommand cmdPlan = new SqlCommand(insertPlanning, conn);
                cmdPlan.Parameters.AddWithValue("@taskId", taskId);
                cmdPlan.Parameters.AddWithValue("@em", loggedUser);
                cmdPlan.Parameters.AddWithValue("@start", startTime.Value);
                cmdPlan.Parameters.AddWithValue("@end", endTime.Value);
                cmdPlan.ExecuteNonQuery();

                MessageBox.Show("Task and Planning added successfully!");
                LoadPlanningData();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding task: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        //  Load planning records
        private void LoadPlanningData()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = @"
                    SELECT P.UUID, T.taskName, T.status, P.startDate, P.endDate
                    FROM Planning P
                    INNER JOIN Task T ON P.taskId = T.taskId
                    WHERE P.email = @em";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@em", loggedUser);

                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        //  When user clicks a row → display data in input boxes
        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgv.Rows[e.RowIndex];

                    // Set fields - display selected row data in the input boxes
                    searchBox.Text = row.Cells["UUID"].Value.ToString();
                    taskNameBox.Text = row.Cells["taskName"].Value.ToString();
                    statusCBox.Text = row.Cells["status"].Value.ToString();
                    startTime.Value = Convert.ToDateTime(row.Cells["startDate"].Value);
                    endTime.Value = Convert.ToDateTime(row.Cells["endDate"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading row data: " + ex.Message);
            }
        }

        //  Update selected record
        private void updateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a record from the table to update.");
                    return;
                }

                if (string.IsNullOrEmpty(taskNameBox.Text) || string.IsNullOrEmpty(statusCBox.Text))
                {
                    MessageBox.Show("Please fill all required fields.");
                    return;
                }

                // Get UUID from selected row
                string uuid = dgv.SelectedRows[0].Cells["UUID"].Value.ToString();

                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = @"
                    UPDATE Task 
                    SET taskName = @taskName, status = @status
                    WHERE taskId = (SELECT taskId FROM Planning WHERE CAST(UUID AS NVARCHAR(50)) = @uuid AND email = @em);
                    
                    UPDATE Planning 
                    SET startDate = @start, endDate = @end
                    WHERE CAST(UUID AS NVARCHAR(50)) = @uuid AND email = @em;";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@taskName", taskNameBox.Text.Trim());
                cmd.Parameters.AddWithValue("@status", statusCBox.Text.Trim());
                cmd.Parameters.AddWithValue("@start", startTime.Value);
                cmd.Parameters.AddWithValue("@end", endTime.Value);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.Parameters.AddWithValue("@em", loggedUser);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Record updated successfully!");
                    LoadPlanningData();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("No record was updated.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating record: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        //  Delete selected record
        private void deleteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a row to delete.");
                    return;
                }

                DataGridViewRow selectedRow = dgv.SelectedRows[0];
                string uuid = selectedRow.Cells["UUID"].Value.ToString();
                string taskName = selectedRow.Cells["taskName"].Value.ToString();

                DialogResult confirm = MessageBox.Show(
                    $"Are you sure you want to delete this task?\n\nTask: {taskName}",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (confirm == DialogResult.No)
                    return;

                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string deleteQuery = "DELETE FROM Planning WHERE CAST(UUID AS NVARCHAR(50)) = @uuid AND email = @em";
                SqlCommand cmd = new SqlCommand(deleteQuery, conn);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.Parameters.AddWithValue("@em", loggedUser);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    MessageBox.Show("Task deleted successfully!");
                    LoadPlanningData();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("No matching record found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting record: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        //  Search by name or status
        private void searchBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = @"
                    SELECT P.UUID, T.taskName, T.status, P.startDate, P.endDate
                    FROM Planning P
                    INNER JOIN Task T ON P.taskId = T.taskId
                    WHERE (T.taskName LIKE @search OR T.status LIKE @search)
                    AND P.email = @em";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@search", "%" + searchBox.Text.Trim() + "%");
                cmd.Parameters.AddWithValue("@em", loggedUser);

                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        //  Clear all input boxes
        private void ClearFields()
        {
            searchBox.Clear();
            taskNameBox.Clear();
            statusCBox.SelectedIndex = -1;
            startTime.Value = DateTime.Now;
            endTime.Value = DateTime.Now;
        }
    }
}