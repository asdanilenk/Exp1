using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SQLite;

namespace Exp1
{
    static class ConnectionManager
    {
        private static string _filename;
        public static string ConnectionString
        {
            get
            {
                return "data source=" + _filename;
            }
        }
        private static SQLiteConnection _connection;
        public static SQLiteConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SQLiteConnection("Data Source = " + Filename);
                    _connection.Open();
                    ExecuteNonQuery("PRAGMA foreign_keys = ON;");
                }
                return _connection;
            }
        }
        public static string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                if (_connection != null) _connection.Close();
                _connection = null;
            }
        }
        public static void CloseConnection()
        {
            if (_connection != null)
                _connection.Close();
            _connection = null;
        }
        public static void ExecuteNonQuery(string sql)
        {
            using (var command = new SQLiteCommand(sql, Connection))
                command.ExecuteNonQuery();
        }
        public static void ExecuteNonQueryIgnoringFK(string sql)
        {
            ExecuteNonQuery("PRAGMA foreign_keys = OFF;");
            ExecuteNonQuery(sql);
            ExecuteNonQuery("PRAGMA foreign_keys = ON;");
        }

    }
}
