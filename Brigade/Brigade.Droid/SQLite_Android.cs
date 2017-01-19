using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Brigade.Abstractions;
using SQLite.Net;
using SQLite.Net.Async;

namespace Brigade.Droid
{
	public class SQLite_Android : ISQLite
	{

		private SQLiteConnectionWithLock _conn;
		private static SQLiteConnection _connection;

		public SQLite_Android()
		{
		}


		private string GetDatabasePath(string sqliteFilename)
		{
			string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
			var path = Path.Combine(documentsPath, sqliteFilename);
			return path;
		}

		public SQLiteAsyncConnection GetAsyncConnection(string databaseName)
		{
			var dbpath = GetDatabasePath(databaseName);

			var platForm = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();

			var connectionFactory = new Func<SQLiteConnectionWithLock>(
				() =>
				{
					if (_conn == null)
					{
						_conn = new SQLiteConnectionWithLock(platForm, new SQLiteConnectionString(dbpath, storeDateTimeAsTicks: false));
					}
					return _conn;
				});

			return new SQLiteAsyncConnection(connectionFactory);
		}

		public SQLite.Net.SQLiteConnection GetConnection(string databaseName)
		{
				
			string path = GetDatabasePath(databaseName);

			var platform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();

			if (_connection == null)
			{
				_connection = new SQLiteConnection(platform, path, storeDateTimeAsTicks: false);
			}
			return _connection;
		}

		public void DeleteDatabase(string databaseName)
		{

			var path = GetDatabasePath(databaseName);

			try
			{
				CloseConnection();
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