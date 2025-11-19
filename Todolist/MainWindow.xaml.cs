using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Todolist.MainWindow;


namespace Todolist
{
    
    public partial class MainWindow : Window
    {
        public class TaskItem
        {
            public int Id { get; set; }
            public string TaskName { get; set; }
            public bool IsCompleted { get; set; } = false;
        }
        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();


        public MainWindow()
        {
            String connectionString = "Data Source=userdata.db;Version=3;";
            SQLiteConnection conn = new SQLiteConnection(connectionString);
            
            InitializeComponent();
            try
            {
                conn.Open();
                SQLiteCommand createTableCommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS todolist (Id INTEGER PRIMARY KEY AUTOINCREMENT, TaskName TEXT, IsCompleted BOOL)", conn);
                createTableCommand.ExecuteNonQuery();
                LoadTasksFromDatabase(conn);
            } catch(Exception ex)
            {
                MessageBox.Show($"Err: {ex.Message}");
            }
            finally
            {
                conn.Close();
            }
            TaskBox.GotFocus += RemoveText;
            TaskBox.LostFocus += AddText;
            ToDoList.ItemsSource = Tasks;
        }
        private void RemoveText(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text == "Enter text here...")
            {
                tb.Text = "";
            }
            
        }

        private void AddText(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "Enter text here...";
            }
                
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string TbContent = TaskBox.Text;
            if (string.IsNullOrEmpty(TbContent) || TbContent == "Enter text here...")
            {
                MessageBox.Show("Please enter something...");
                return;
            }

            var newTask = new TaskItem { TaskName = TbContent, IsCompleted = false };

            string connectionString = "Data Source=userdata.db;Version=3;";
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string insertSql = "INSERT INTO todolist (TaskName, IsCompleted) VALUES (@name, @completed)";
                    using (SQLiteCommand insertCommand = new SQLiteCommand(insertSql, conn))
                    {
                        insertCommand.Parameters.AddWithValue("@name", newTask.TaskName);
                        insertCommand.Parameters.AddWithValue("@completed", newTask.IsCompleted);
                        insertCommand.ExecuteNonQuery();
                    }
                    using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                    {
                        newTask.Id = Convert.ToInt32(idCommand.ExecuteScalar());
                    }

                    Tasks.Add(newTask);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Err: {ex.Message}");
                }
            }

            TaskBox.Text = "Enter text here...";
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            TaskItem task = btn.Tag as TaskItem;
            if (task != null)
            {
                Tasks.Remove(task);

                string connectionString = "Data Source=userdata.db;Version=3;";
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string deleteSql = "DELETE FROM todolist WHERE Id = @id";
                        using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteSql, conn))
                        {
                            deleteCommand.Parameters.AddWithValue("@id", task.Id);
                            deleteCommand.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting task: {ex.Message}");
                    }
                }
            }
        }
        private void LoadTasksFromDatabase(SQLiteConnection conn)
        {
            try
            {
                string query = "SELECT Id, TaskName, IsCompleted FROM todolist";
                SQLiteCommand command = new SQLiteCommand(query, conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Tasks.Add(new TaskItem
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        TaskName = reader["TaskName"].ToString(),
                        IsCompleted = reader["IsCompleted"] != DBNull.Value && Convert.ToBoolean(reader["IsCompleted"])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}");
            }
        }

    }
}