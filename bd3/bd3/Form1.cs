using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;


namespace bd3
{
    public partial class Form1 : Form
    {
        public static string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database1.mdb;";
        public OleDbConnection connection;
        public Form1()
        {
            InitializeComponent();

            InitializeDatabaseConnection();
        }

        private void InitializeDatabaseConnection()
        {
            try
            {
                connection = new OleDbConnection(connectionString);
                this.Load += new EventHandler(Form1_Load);
                this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации подключения: {ex.Message}");
            }
        }

        private void LoadData()
        {
            string query = "SELECT * FROM table1";
            OleDbCommand command = new OleDbCommand(query, connection);
            OleDbDataReader reader = command.ExecuteReader();

            DataTable table = new DataTable();
            table.Load(reader);
            dataGridView1.DataSource = table;

            reader.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                connection.Open();
                LoadData(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string query = "SELECT MAX(experience) FROM table1";
            OleDbCommand command = new OleDbCommand(query, connection);
            object result = command.ExecuteScalar();
            textBox1.Text = $"Максимальный стаж: {result}";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Введите букву для поиска");
                return;
            }

            string letter = textBox1.Text.Substring(0, 1);
            string query = "SELECT surname FROM table1 WHERE surname LIKE @letter + '%'";
            OleDbCommand command = new OleDbCommand(query, connection);
            command.Parameters.AddWithValue("@letter", letter);

            OleDbDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            dataGridView1.DataSource = table;
            reader.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Введите диапазон зарплат в формате 'мин-макс' (например: 50000-70000)");
                return;
            }

            string[] range = textBox1.Text.Split('-');
            if (range.Length != 2)
            {
                MessageBox.Show("Введите диапазон в формате 'мин-макс' (например: 50000-70000)");
                return;
            }

            try
            {
                string query = "SELECT surname FROM table1 WHERE salary BETWEEN @min AND @max";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@min", int.Parse(range[0]));
                command.Parameters.AddWithValue("@max", int.Parse(range[1]));

                OleDbDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(reader);
                dataGridView1.DataSource = table;
                reader.Close();

                MessageBox.Show($"Найдено сотрудников с зарплатой от {range[0]} до {range[1]}: {table.Rows.Count}");
            }
            catch (FormatException)
            {
                MessageBox.Show("Ошибка формата! Используйте числа (например: 50000-70000)");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string addColumnQuery = @"ALTER TABLE table1 ADD COLUMN annual_salary CURRENCY";
            OleDbCommand addColumnCommand = new OleDbCommand(addColumnQuery, connection);
            addColumnCommand.ExecuteNonQuery();

            string updateQuery = "UPDATE table1 SET annual_salary = salary * 12";
            OleDbCommand updateCommand = new OleDbCommand(updateQuery, connection);
            updateCommand.ExecuteNonQuery();

            LoadData();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Введите название отдела для дублирования (IT или HR)");
                return;
            }

            string department = textBox1.Text.ToUpper();

            string checkQuery = "SELECT COUNT(*) FROM table1 WHERE department = @department";
            OleDbCommand checkCommand = new OleDbCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@department", department);
            int count = (int)checkCommand.ExecuteScalar();

            if (count == 0)
            {
                MessageBox.Show($"Отдел '{department}' не найден! Доступные отделы: IT, HR");
                return;
            }

            string query = "INSERT INTO table1 (surname, department, salary, experience) " +
                           "SELECT surname + '_copy', department, salary, experience FROM table1 " +
                           "WHERE department = @department";
            OleDbCommand command = new OleDbCommand(query, connection);
            command.Parameters.AddWithValue("@department", department);

            int rowsAffected = command.ExecuteNonQuery();
            MessageBox.Show($"Дублировано сотрудников отдела '{department}': {rowsAffected} записей");
            LoadData();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить всех сотрудников с ID > 6?",
                                        "Подтверждение удаления",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                string query = "DELETE FROM table1 WHERE id > 6";
                OleDbCommand command = new OleDbCommand(query, connection);

                int rowsAffected = command.ExecuteNonQuery();
                MessageBox.Show($"Удалено сотрудников: {rowsAffected}");
                LoadData(); 
            }
        }

        
    }
}
