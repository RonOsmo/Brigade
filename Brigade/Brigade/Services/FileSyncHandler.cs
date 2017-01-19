using System;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Brigade.Abstractions;

namespace Brigade.Services
{
	public class FileSyncHandler : IFileSyncHandler
	{
		private readonly IMobilePlatform _mobilePlatform;
		private string _dataFilesPath;

		public FileSyncHandler()
		{
			_mobilePlatform = DependencyService.Get<IMobilePlatform>();
		}

		public async Task InitialiseAsync()
		{
			_dataFilesPath = await _mobilePlatform.GetDataFilesPath();
		}

		public Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
		{
			return _mobilePlatform.GetFileDataSource(metadata);
		}

		public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
		{
			try
			{
				if (action == FileSynchronizationAction.Delete)
				{
					await FileHelper.DeleteLocalFileAsync(file, _dataFilesPath);
				}
				else
				{ // Create or update - download large format image by looking for 'lg' in the StoreUri parameter
					Trace.WriteLine(string.Format("File - storeUri: {1}", file.Name, file.StoreUri));

					if (file.StoreUri.Contains("lg"))
					{
						await DownloadFileAsync(file);
					}
				}
			}
			catch (Exception e)
			{ // should catch WrappedStorageException, but this type is internal in the Storage SDK!
				Trace.WriteLine("Exception while downloading blob, blob probably does not exist: " + e);
			}
		}


	}
}
