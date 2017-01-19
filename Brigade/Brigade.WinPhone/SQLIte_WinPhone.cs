using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brigade.Abstractions;
using SQLite.Net;
using SQLite.Net.Async;

namespace Brigade.WinPhone
{
	public class SQLite_WinPhone : ISQLite
	{

		private static SQLiteConnectionWithLock _conn;

		public SQLite_WinPhone() { }

		private static Object _connectionLock = new Object();


		public SQLite.Net.Async.SQLiteAsyncConnection GetAsyncConnection(string databaseName)
		{
			lock (_connectionLock)
			{
				var sqliteFilename = databaseName;
				string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, sqliteFilename);

				var platform = new SQLite.Net.Platform.WindowsPhone8.SQLitePlatformWP8();

				var connectionString = new SQLiteConnectionString(path, storeDateTimeAsTicks: false);

				var connectionFactory = new Func<SQLiteConnectionWithLock>(
					() =>
					{
						if (_conn == null)
						{
							_conn =
								new SQLiteConnectionWithLock(platform,
									connectionString);

						}
						return _conn;
					});

				return new SQLiteAsyncConnection(connectionFactory);
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
			if (_conn != null)
			{

				_conn.Close();
				_conn.Dispose();

				_conn = null;

				// Must be called as the disposal of the connection is not released until the GC runs.
				GC.Collect();
				GC.WaitForPendingFinalizers();

			}

		}
	}
}
