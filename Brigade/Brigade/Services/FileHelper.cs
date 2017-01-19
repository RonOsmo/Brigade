using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xamarin.Forms;
using PCLStorage;
using Brigade.Abstractions;

namespace Brigade.Services
{
	public class FileHelper<T> where T : IEntityId
	{
		private static string rootFilePath;

		private static async Task<string> GetRootFilesPathAsync()
		{
			if (string.IsNullOrWhiteSpace(rootFilePath))
			{
				IMobilePlatform platform = DependencyService.Get<IMobilePlatform>();
				rootFilePath = Path.Combine(await platform.GetDataFilesPath(), typeof(T).Name);
			}
			return rootFilePath;
		}

		public static async Task<string> CopyItemFileAsync(T item, string filePath)
		{
			IFolder localStorage = FileSystem.Current.LocalStorage;

			string fileName = Path.GetFileName(filePath);
			string targetPath = await GetLocalFilePathAsync(item.Id, fileName);

			var sourceFile = await localStorage.GetFileAsync(filePath);
			var sourceStream = await sourceFile.OpenAsync(PCLStorage.FileAccess.Read);

			var targetFile = await localStorage.CreateFileAsync(targetPath, CreationCollisionOption.ReplaceExisting);

			using (var targetStream = await targetFile.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
			{
				await sourceStream.CopyToAsync(targetStream);
			}

			return targetPath;
		}

		public static async Task<string> GetLocalFilePathAsync(string itemId, string fileName)
		{
			string[] seps = { "," };
			string[] parts = itemId.Split(seps, System.StringSplitOptions.RemoveEmptyEntries);
			string recordFilesPath = Path.Combine(await GetRootFilesPathAsync(), parts[0]);

			if (parts.Length == 2)
				recordFilesPath = Path.Combine(recordFilesPath, parts[1]);

			var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(recordFilesPath);
			if (checkExists == ExistenceCheckResult.NotFound)
			{
				await FileSystem.Current.LocalStorage.CreateFolderAsync(recordFilesPath, CreationCollisionOption.ReplaceExisting);
			}

			return Path.Combine(recordFilesPath, fileName);
		}

		public static async Task DeleteLocalFileAsync(Microsoft.WindowsAzure.MobileServices.Files.MobileServiceFile fileName)
		{
			string localPath = await GetLocalFilePathAsync(fileName.ParentId, fileName.Name);
			var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(localPath);

			if (checkExists == ExistenceCheckResult.FileExists)
			{
				var file = await FileSystem.Current.LocalStorage.GetFileAsync(localPath);
				await file.DeleteAsync();
			}
		}
	}
}
