
namespace Brigade.Services
{

#if AZURE
using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Reflection;
using Brigade.Abstractions;

    public class Batch
    {
        public TableBatchOperation Op { get; set; }
        public CloudTable Table { get; set; }
        public List<Type> OpTypes { get; set; } 
        public Task<IList<TableResult>> Results { get; set; }
    }

    public class DataAccessLayer
    {
        private Dictionary<string, CloudTable> tables = new Dictionary<string, CloudTable>();
        private bool IsBatched = false;
        private Dictionary<string, Batch> Batches = new Dictionary<string, Batch>();
        private ILoginService loginService;
        private CloudStorageAccount storageAccount = null;

        public DataAccessLayer(ILoginService loginSVC)
        {
            loginService = loginSVC;
            storageAccount = CreateStorageAccountFromConnectionString(loginService.StorageConnectionString);
        }

        private CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

        private async Task<CloudTable> CreateTableAsync<T>() where T : TableEntity
        {
            Type t = typeof(T);
            System.ComponentModel.DataAnnotations.Schema.TableAttribute tableAttribute =
                (System.ComponentModel.DataAnnotations.Schema.TableAttribute)Attribute.GetCustomAttribute(t, typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute));
            if (tableAttribute == null)
            {
                throw new ArgumentException($"Class {t.Name} does not declare its table name");
            }

            string name = tableAttribute.Name;

            if (tables.ContainsKey(name))
            {
                return tables[tableAttribute.Name];
            }
            else
            {
                // Create a table client for interacting with the table service
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                Console.WriteLine("1. Create a Table for the demo");

                // Create a table client for interacting with the table service 
                CloudTable table = tableClient.GetTableReference(name);
                try
                {
                    if (await table.CreateIfNotExistsAsync())
                    {
                        Console.WriteLine($"Created Table named: {name}");
                    }
                    else
                    {
                        Console.WriteLine($"Table {name} already exists");
                    }
                }
                catch (StorageException)
                {
                    Console.WriteLine("If you are running with the default configuration please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                    Console.ReadLine();
                    throw;
                }
                tables.Add(name, table);
                return table;
            }
        }

        public void BatchStart()
        {
            IsBatched = true;
        }

        public async Task<List<TableEntity>> BatchExecuteAsync()
        {
            List<TableEntity> results = new List<TableEntity>();

            // execute batches in parallel
            foreach (Batch batch in Batches.Values)
            {
                batch.Results = batch.Table.ExecuteBatchAsync(batch.Op);
            }

            // await & convert results from object to the original type
            foreach (Batch batch in Batches.Values)
            {
                await batch.Results;
                var opTypesEnumerator = batch.OpTypes.GetEnumerator();
                foreach (var result in batch.Results.Result)
                {
                    opTypesEnumerator.MoveNext();
                    Type t = opTypesEnumerator.Current;
                    TableEntity res = null;
                    if (result != null)
                    {
                        res = Convert.ChangeType(result, t) as TableEntity;
                    }
                    results.Add(res);
                }
            }

            // reset the batches now they are used up
            Batches.Clear();
            IsBatched = false;

            return results;
        }

        public async Task DeleteEntityAsync<T>(T entity) where T : TableEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            CloudTable table = await CreateTableAsync<T>();

            if (IsBatched)
            {
                Batch batch;
                if (!Batches.ContainsKey(entity.PartitionKey))
                {
                    Batches.Add(entity.PartitionKey, batch = new Batch { Op = new TableBatchOperation(), Table = table });
                    batch.OpTypes = new List<Type>();
                }
                else
                {
                    batch = Batches[entity.PartitionKey];
                }
                batch.Op.Delete(entity);
                batch.OpTypes.Add(null);
            }
            else
            {
                // Create the InsertOrReplace  TableOperation
                TableOperation operation = TableOperation.Delete(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(operation);
            }
        }

        public async Task<T> InsertEntityAsync<T>(T entity) where T : TableEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            CloudTable table = await CreateTableAsync<T>();

            if (IsBatched)
            {
                Batch batch;
                if (!Batches.ContainsKey(entity.PartitionKey))
                {
                    Batches.Add(entity.PartitionKey, batch = new Batch { Op = new TableBatchOperation(), Table = table });
                    batch.OpTypes = new List<Type>();
                }
                else
                {
                    batch = Batches[entity.PartitionKey];
                }
                batch.Op.Insert(entity);
                batch.OpTypes.Add(typeof(T));
                return entity;
            }
            else
            {
                // Create the Insert  TableOperation
                TableOperation insertOperation = TableOperation.Insert(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOperation);

                return result.Result as T;
            }
        }

        public async Task<T> InsertOrMergeEntityAsync<T>(T entity) where T : TableEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            CloudTable table = await CreateTableAsync<T>();

            if (IsBatched)
            {
                Batch batch;
                if (!Batches.ContainsKey(entity.PartitionKey))
                {
                    Batches.Add(entity.PartitionKey, batch = new Batch { Op = new TableBatchOperation(), Table = table });
                    batch.OpTypes = new List<Type>();
                }
                else
                {
                    batch = Batches[entity.PartitionKey];
                }
                batch.Op.InsertOrMerge(entity);
                batch.OpTypes.Add(typeof(T));
                return entity;
            }
            else
            {
                // Create the InsertOrReplace  TableOperation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);

                return result.Result as T;
            }
        }

        public async Task<T> InsertOrReplaceEntityAsync<T>(T entity) where T : TableEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            CloudTable table = await CreateTableAsync<T>();

            if (IsBatched)
            {
                Batch batch;
                if (!Batches.ContainsKey(entity.PartitionKey))
                {
                    Batches.Add(entity.PartitionKey, batch = new Batch { Op = new TableBatchOperation(), Table = table });
                    batch.OpTypes = new List<Type>();
                }
                else
                {
                    batch = Batches[entity.PartitionKey];
                }
                batch.Op.InsertOrReplace(entity);
                batch.OpTypes.Add(typeof(T));
                return entity;
            }
            else
            {
                // Create the InsertOrReplace  TableOperation
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrReplaceOperation);

                return result.Result as T;
            }
        }

        public async Task PartitionScanAsync<T>(string partitionKey) where T : TableEntity
        {
            CloudTable table = await CreateTableAsync<T>();

            TableQuery<T> partitionScanQuery = new TableQuery<T>().Where
                (TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            TableContinuationToken token = null;
            List<T> results = new List<T>();

            // Page through the results
            do
            {
                TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync<T>(partitionScanQuery, token);
                token = segment.ContinuationToken;
                //foreach (T entity in segment)
                //{
                //    Console.WriteLine("Customer: {0},{1}\t{2}\t{3}", entity.PartitionKey, entity.RowKey, entity.Email, entity.PhoneNumber);
                //}
            }
            while (token != null);
        }

        public async Task<T> ReplaceAsync<T>(T entity) where T : TableEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            CloudTable table = await CreateTableAsync<T>();

            if (IsBatched)
            {
                Batch batch;
                if (!Batches.ContainsKey(entity.PartitionKey))
                {
                    Batches.Add(entity.PartitionKey, batch = new Batch { Op = new TableBatchOperation(), Table = table });
                    batch.OpTypes = new List<Type>();
                }
                else
                {
                    batch = Batches[entity.PartitionKey];
                }
                batch.Op.Replace(entity);
                batch.OpTypes.Add(typeof(T));
                return null;
            }
            else
            {
                // Create the InsertOrReplace  TableOperation
                TableOperation replaceOperation = TableOperation.Replace(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(replaceOperation);

                return result.Result as T;
            }
        }

        public async Task<T> RetrieveAsync<T>(string partitionKey, string rowKey) where T : TableEntity
        {
            if (partitionKey == null)
            {
                throw new ArgumentNullException(nameof(partitionKey));
            }
            if (rowKey == null)
            {
                throw new ArgumentException(nameof(rowKey));
            }

            CloudTable table = await CreateTableAsync<T>();

            if (IsBatched)
            {
                Batch batch;
                if (!Batches.ContainsKey(partitionKey))
                {
                    Batches.Add(partitionKey, batch = new Batch { Op = new TableBatchOperation(), Table = table });
                    batch.OpTypes = new List<Type>();
                }
                else
                {
                    batch = Batches[partitionKey];
                }
                batch.Op.Retrieve(partitionKey, rowKey);
                batch.OpTypes.Add(typeof(T));
                return null;
            }
            else
            {
                // Create the InsertOrReplace  TableOperation
                TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(retrieveOperation);

                return result.Result as T;
            }
        }

    }
#endif

}
