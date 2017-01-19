﻿using PCLStorage;
using System.Threading.Tasks;

namespace Brigade
{
	public class FileHelper
	{
		public static async Task<string> SaveStreamAsync(string itemId, string filename, System.IO.Stream sourceStream, string dataFilesPath)
		{
			IFolder localStorage = FileSystem.Current.LocalStorage;

			string targetPath = await GetLocalFilePathAsync(itemId, filename, dataFilesPath);
			var targetFile = await localStorage.CreateFileAsync(targetPath, CreationCollisionOption.ReplaceExisting);

			using (var targetStream = await targetFile.OpenAsync(FileAccess.ReadAndWrite))
			{
				await sourceStream.CopyToAsync(targetStream);
			}

			return targetPath;
		}

		public static async Task<string> CopyFileAsync(string itemId, string filePath, string dataFilesPath)
		{
			var sourceFile = await FileSystem.Current.LocalStorage.GetFileAsync(filePath);
			var sourceStream = await sourceFile.OpenAsync(FileAccess.Read);

			return await SaveStreamAsync(itemId, itemId/* + fileExt*/, sourceStream, dataFilesPath);
		}

		public static async Task<string> GetLocalFilePathAsync(string itemId, string fileName, string dataFilesPath)
		{
			string recordFilesPath = System.IO.Path.Combine(dataFilesPath, itemId);

			var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(recordFilesPath);
			if (checkExists == ExistenceCheckResult.NotFound)
			{
				await FileSystem.Current.LocalStorage.CreateFolderAsync(recordFilesPath, CreationCollisionOption.ReplaceExisting);
			}

			return System.IO.Path.Combine(recordFilesPath, fileName);
		}

		public static async Task DeleteLocalFileAsync(string fullPath)
		{
			var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(fullPath);

			if (checkExists == ExistenceCheckResult.FileExists)
			{
				var file = await FileSystem.Current.LocalStorage.GetFileAsync(fullPath);
				await file.DeleteAsync();
			}
		}

		public static async Task DeleteLocalFileAsync(Microsoft.WindowsAzure.MobileServices.Files.MobileServiceFile fileName, string dataFilesPath)
		{
			string localPath = await GetLocalFilePathAsync(fileName.ParentId, fileName.Name, dataFilesPath);
			await DeleteLocalFileAsync(localPath);
		}
	}
}