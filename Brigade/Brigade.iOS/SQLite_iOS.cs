using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Brigade.Abstractions;
using SQLite.Net;
using SQLite.Net.Async;

namespace Brigade.iOS
{
	public class SQLite_iOS : ISQLite
	{
		private SQLiteConnectionWithLock _conn;
		private static SQLiteConnection _connection;

		public SQLite_iOS()
		{
		}

		private string GetDatabasePath(string sqliteFilename)
		{
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
			string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder
			var libraryPathFull = Path.Combine(libraryPath, sqliteFilename);
			return libraryPathFull;
		}

		public SQLiteAsyncConnection GetAsyncConnection(string databaseName)
		{
			var dbpath = GetDatabasePath(databaseName);

			var platForm = new SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS();

			var connectionFactory = new Func<SQLiteConnectionWithLock>(
				() =>
				{
					if (_conn == null)
					{
						_conn = new SQLiteConnectionWithLock(platForm, new SQLiteConnectionString(dbpath, storeDateTimeAsTicks: false));
					}
					return _conn;
				});
			var asyncConnection = new SQLiteAsyncConnection(connectionFactory);

			return asyncConnection;
		}

		public SQLite.Net.SQLiteConnection GetConnection(string databaseName)
		{

			string path = GetDatabasePath(databaseName);

			var platform = new SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS();

			if (_connection == null)
			{
				_connection = new SQLiteConnection(platform, path, storeDateTimeAsTicks: false);
			}
			return _connection;
		}


		public void DeleteDatabase(string databaseName)
		{
			try
			{
				var path = GetDatabasePath(databaseName);

				try
				{
					if (_conn != null)
					{
						_conn.Close();

					}
				}
				catch (Exception ex)
				{
					// Best effort close. No need to worry if throws an exception
				}

				if (File.Exists(path))
				{
					File.Delete(path);
				}

				_conn = null;

			}
			catch (Exception ex)
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
