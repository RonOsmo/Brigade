using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;
using System.Threading;
using Osmosys.Stocktake.Models;

namespace Osmosys.Stocktake.Data
{
    public class Repository
	{
		
		readonly CloudTable _stockTable;
		readonly List<string> _stockColumns = new List<string>();

		public Repository()
		{
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

			// Create the table client.
			CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

			// Create the CloudTable object that represents the "asset" table.
			_stockTable = tableClient.GetTableReference("stock");

			_stockColumns.Add("RowKey");
			_stockColumns.Add("PartitionKey");
			_stockColumns.Add("ETAG");
			_stockColumns.Add("AssetId");
			_stockColumns.Add("MissingDate");
			_stockColumns.Add("MissingByUserId");
			_stockColumns.Add("MissingAtLocation");
			_stockColumns.Add("LastSeenDate");
			_stockColumns.Add("LastSeenByUserId");
			_stockColumns.Add("LastSeenAtLocation");
			_stockColumns.Add("Status");
			_stockColumns.Add("IsContainer");
		}

		public async Task AssetInsertAsync(StockDto stockDto)
		{
			DynamicTableEntity entity = ToEntity(stockDto);
			TableOperation op = TableOperation.Insert(entity);
			await _stockTable.ExecuteAsync(op);
		}

		private DynamicTableEntity ToEntity(StockDto stockDto)
		{
			DynamicTableEntity entity = new DynamicTableEntity(stockDto.Id, stockDto.BrigadeId + (!string.IsNullOrWhiteSpace(stockDto.ContainerId) ? "." + stockDto.ContainerId : ""));
			foreach(string column in _stockColumns)
			{
				switch (column)
				{
					case "RowKey":
					case "PartitionKey":
					case "ETAG":
						break;
					case "AssetId":
						if (!string.IsNullOrWhiteSpace(stockDto.AssetId))
							entity.Properties[column] = new EntityProperty(stockDto.AssetId);
						break;
					case "MissingDate":
						if (stockDto.MissingDate.HasValue)
							entity.Properties[column] = new EntityProperty(stockDto.MissingDate);
						break;
					case "MissingByUserId":
						if (!string.IsNullOrWhiteSpace(stockDto.MissingByUserId))
							entity.Properties[column] = new EntityProperty(stockDto.MissingByUserId);
						break;
					case "MissingAtLocation":
						if (!string.IsNullOrWhiteSpace(stockDto.MissingAtLocation))
							entity.Properties[column] = new EntityProperty(stockDto.MissingAtLocation);
						break;
					case "LastSeenDate":
						if (stockDto.LastSeenDate.HasValue)
							entity.Properties[column] = new EntityProperty(stockDto.LastSeenDate);
						break;
					case "LastSeenByUserId":
						if (!string.IsNullOrWhiteSpace(stockDto.LastSeenByUserId))
							entity.Properties[column] = new EntityProperty(stockDto.MissingByUserId);
						break;
					case "LastSeenAtLocation":
						if (!string.IsNullOrWhiteSpace(stockDto.LastSeenAtLocation))
							entity.Properties[column] = new EntityProperty(stockDto.MissingByUserId);
						break;
					case "Status":
						entity.Properties[column] = new EntityProperty(stockDto.Status.ToString());
						break;
					case "IsContainer":
						if (stockDto.IsContainer)
							entity.Properties[column] = new EntityProperty(stockDto.IsContainer);
						break;
				}
			}
			return entity;
		}

		public async Task<List<StockDto>> GetStocksByPartitionAsync(string partitionKey)
		{
			List<StockDto> stocks = new List<StockDto>();
			var query = new TableQuery<DynamicTableEntity>()
				.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey))
				.Select(_stockColumns);

			var results = await RepositoryHelpers.ExecuteQueryAsync(_stockTable, query);
			if (results.Any())
			{
				foreach (var entity in results)
				{
					stocks.Add(ToStock(entity));
				}
			}
			return stocks;
		}

		private StockDto ToStock(DynamicTableEntity entity)
		{
			StockDto stockDto = new Models.StockDto();
			string[] seps = { "." };
			foreach (var key in entity.Properties.Keys)
			{
				switch (key)
				{
					case "RowKey":
						stockDto.Id = entity.Properties[key].StringValue;
						break;
					case "PartionKey":
						string[] parts = entity.Properties[key].StringValue.Split(seps, StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length >= 2)
							stockDto.BrigadeId = string.Join(".", parts.Skip(1));
						stockDto.ContainerId = parts[0];
						break;
					case "AssetId":
						stockDto.AssetId = entity.Properties[key].StringValue;
						break;
					case "ETAG":
						stockDto.Etag = entity.Properties[key].StringValue;
						break;
					case "MissingDate":
						stockDto.MissingDate = entity.Properties[key].DateTime;
						break;
					case "MissingByUserId":
						stockDto.Id = entity.Properties[key].StringValue;
						break;
					case "MissingAtLocation":
						stockDto.Id = entity.Properties[key].StringValue;
						break;
					case "LastSeenDate":
						stockDto.LastSeenDate = entity.Properties[key].DateTime;
						break;
					case "LastSeenByUserId":
						stockDto.Id = entity.Properties[key].StringValue;
						break;
					case "LastSeenAtLocation":
						stockDto.Id = entity.Properties[key].StringValue;
						break;
					case "Status":
						StockStatus status;
						if (System.Enum.TryParse<StockStatus>(entity.Properties["Status"].StringValue, out status))
							stockDto.Status = status;
						break;
					case "IsContainer":
						bool? isContainer = entity.Properties["IsContainer"].BooleanValue;
						stockDto.IsContainer = isContainer.HasValue ? isContainer.Value : false;
						break;
				}
			}
			return stockDto;
		}
	}

	public static class RepositoryHelpers
	{ 
		public static async Task<IList<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query, CancellationToken ct = default(CancellationToken)) where T : ITableEntity, new()
		{
			var runningQuery = new TableQuery<T>()
			{
				FilterString = query.FilterString,
				SelectColumns = query.SelectColumns
			};

			var items = new List<T>();
			TableContinuationToken token = null;

			do
			{
				runningQuery.TakeCount = query.TakeCount - items.Count;

				TableQuerySegment<T> seg = await table.ExecuteQuerySegmentedAsync<T>(runningQuery, token, ct);
				token = seg.ContinuationToken;
				items.AddRange(seg);

			} while (token != null && !ct.IsCancellationRequested && (query.TakeCount == null || items.Count < query.TakeCount.Value));

			return items;
		}

	}

	
}
