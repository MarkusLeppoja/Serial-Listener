using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;
using System.IO.Ports;

namespace SerialListener
{
    internal class Program
    {
        private static SerialPort _serialPort;
        

        static void Main(string[] args)
        {
            Console.WriteLine("Serial Listener!");

            // Serial startup
            _serialPort.DataReceived +=_serialPort_DataReceived;
            while (!connectToSerial()){ }

            connectToDB();


            while (true)
            {
                postData(0, float.Parse(Console.ReadLine()));
            }
        }

        private static void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            postData(int.Parse(_serialPort.ReadLine()), 0);
        }

        static bool connectToSerial()
        {
            _serialPort = new SerialPort();

            Console.WriteLine("Select from available serial ports:");
            foreach (var port in SerialPort.GetPortNames())
            {
                Console.WriteLine(port);
            }

            string selectedPort = Console.ReadLine();

            try
            {
                _serialPort.PortName = selectedPort;
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                return false;
            }

            Console.WriteLine($"Serial port {selectedPort} opened.");

            return true;
        }

        private static MySqlConnection _dbConnection;
        private static MySqlCommand _dbCommand;
        private static string connection_data = "server=localhost;user=root;database=data_visualiser_website;port=3306;";
        static bool connectToDB()
        {
            // Generate and open new MySQL connection
            _dbConnection = new MySqlConnection(connection_data);
            _dbConnection.Open();

            // Generate new command instance
            _dbCommand = new MySqlCommand();

            return _dbConnection.State.ToString() == "Open" ? true : false;
        }

        static void postData(float parameter_1, float parameter_2)
        {
            string timeStamp = GetTimeData();
            string query = "";

            if (parameter_1 == 0 && parameter_2 != 0)
            {
                query = $"insert into sugar_motor_db(datetime,thermistor_2) values('{timeStamp}','{parameter_2}')"; 
            }

            if (parameter_1 != 0 && parameter_2 == 0)
            {
                query = $"insert into sugar_motor_db(datetime,thermistor_1) values('{timeStamp}','{parameter_1}')";
            }

            if (!string.IsNullOrEmpty(query))
                PostDataToMySQL(query);
        }

        private static bool PostDataToMySQL(string query)
        {
            if (query == null) return false;
            _dbCommand.Connection = _dbConnection;
            _dbCommand.CommandText = query;
            return _dbCommand.ExecuteNonQuery() == 1 ? true : false;
        }

        private static string GetTimeData()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
