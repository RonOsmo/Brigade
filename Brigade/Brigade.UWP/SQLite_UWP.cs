using System;
using System.IO;
using Windows.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brigade.Abstractions;
using SQLite.Net;
using SQLite.Net.Async;

namespace Brigade.UWP
{
	public class SQLite_WinPhone : ISQLite
	{

		private static SQLiteConnectionWithLock _conn;
		private static SQLiteConnection _connection;

		public SQLite_WinPhone() { }

		private static Object _connectionLock = new Object();


		public SQLite.Net.Async.SQLiteAsyncConnection GetAsyncConnection(string databaseName)
		{
			lock (_connectionLock)
			{
				var sqliteFilename = databaseName;
				string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, sqliteFilename);

				var platform = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();

				var connectionString = new SQLiteConnectionString(path, storeDateTimeAsTicks: false);

				var connectionFactory = new Func<SQLiteConnectionWithLock>(
					() =>
					{
						if (_conn == null)
						{
							_conn = new SQLiteConnectionWithLock(platform, connectionString);

						}
						return _conn;
					});

				return new SQLiteAsyncConnection(connectionFactory);
			}
		}

		public SQLite.Net.SQLiteConnection GetConnection(string databaseName)
		{
			lock (_connectionLock)
			{
				var sqliteFilename = databaseName;
				string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, sqliteFilename);

				var platform = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();

				if (_connection == null)
				{
					_connection = new SQLiteConnection(platform, path, storeDateTimeAsTicks: false);

				}
				return _connection;
			}
		}

		public void DeleteDatabase(string databaseName)
		{
			try
			{
				string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);

				CloseConnection();

				if (File.Exists(path))
				{
					File.Delete(path);
				}

			}
			catch
			{
				throw;
			}
		}

		public void CloseConnection()
		{
			bool callGC = false;
			if (_connection != null)
			{
				_connection.Close();
				_connection.Dispose();
				_connection = null;
				callGC = true;
			}
			if (_conn != null)
			{

				_conn.Close();
				_conn.Dispose();
				_conn = null;
				callGC = true;
			}
			if (callGC)
			{ 
				// Must be called as the disposal of the connection is not released until the GC runs.
				GC.Collect();
				GC.WaitForPendingFinalizers();

			}

		}
	}
}