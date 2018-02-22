using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;


namespace AdminPanel
{
    public class AzureHelper
    {
        public AzureHelper(string connectionString)
        {
            m_connectionString = connectionString;
            Account = CloudStorageAccount.Parse(connectionString);
            BlobClient = Account.CreateCloudBlobClient();
            TableClient = Account.CreateCloudTableClient();
        }
        /// <summary> Account connection string </summary>
        private string m_connectionString;
        /// <summary> Azure cloud storage account </summary>
        public CloudStorageAccount Account;
        /// <summary> Azure Blob client for storage account </summary>
        public CloudBlobClient BlobClient;
        /// <summary> Azure Table client for storage account </summary>
        public CloudTableClient TableClient;

        #region Blob
        // Creates a Blob container of a specific name. (Currently not used but could be for setting up)
        public async Task CreateBlobContainer(string name)
        {
            CloudBlobContainer container = BlobClient.GetContainerReference(name);
            await container.CreateIfNotExistsAsync();
        }

        // Pushes a blob to the container of choice (This is a blob with no content and the data is all stored as a parsable string in the name)
        public async Task PushBlob(string containerName, string contentString)
        {
            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(contentString);
            await blockBlob.UploadTextAsync(contentString);
        }

        // Replaces a Specified blob with another in a specified container
        public async Task ReplaceBlob(string containerName, string contentString, string newContentString)
        {
            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(contentString);
            await blockBlob.DeleteAsync();
            blockBlob = container.GetBlockBlobReference(newContentString);
            await blockBlob.UploadTextAsync(newContentString);
        }

        // Replaces a Specified blob with another in a specified container
        public async Task ReplaceBlobContents(string containerName, string blobname, string newContentString)
        {
            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobname);
            await blockBlob.DeleteAsync();
            blockBlob = container.GetBlockBlobReference(blobname);
            await blockBlob.UploadTextAsync(newContentString);
        }

        // Replaces a Specified blob with another in a specified container
        public async Task ReplaceBlobName(string containerName, string oldBlobName, string newBlobName, string newContentString)
        {
            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(oldBlobName);
            await blockBlob.DeleteAsync();
            blockBlob = container.GetBlockBlobReference(newBlobName);
            await blockBlob.UploadTextAsync(newContentString);
        }

        // Pushes a data blob from a specific path on current system to a container of choice (This can be any data and the name is simply a name)
        public async Task PushFileBlob(string containerName, string contentString, string path)
        {
            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(contentString);

            await blockBlob.UploadFromFileAsync(path);
        }

        // Downloads a blob from a specified container with specified name and places it at specified path
        public async Task BlobToFile(string containerName, string contentString, string path)
        {
            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(contentString);

            await blockBlob.DownloadToFileAsync(path, System.IO.FileMode.Create);
        }

        // Deletes a blob of specified name in specified container
        public async Task DeleteBlob(string containerName, string contentString)
        {
            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(contentString);
            await blockBlob.DeleteAsync();
        }
        #endregion

        #region Table
        // Creates a table with specified name
        public async Task CreateTable(string name)
        {
            CloudTable table = TableClient.GetTableReference(name);
            await table.CreateIfNotExistsAsync();
        }

        // Adds a table entity to a specific table
        public async Task AddEvent(string tableName, CalendarEvent newEvent)
        {
            CloudTable table = TableClient.GetTableReference(tableName);
            TableOperation insert = TableOperation.Insert(newEvent);
            await table.ExecuteAsync(insert);
        }

        // Adds a table entity to a specific table
        public async Task AddQuestion(string tableName, TriviaQuestion question)
        {
            CloudTable table = TableClient.GetTableReference(tableName);
            TableOperation insert = TableOperation.Insert(question);
            await table.ExecuteAsync(insert);
        }

        // Adds an array of table entities to a specific table
        public async Task AddRangeToTable(string tableName, CalendarEvent[] newEvents)
        {
            CloudTable table = TableClient.GetTableReference(tableName);
            TableBatchOperation batch = new TableBatchOperation();
            for(int i = 0; i < newEvents.Length; ++i)
            {
                batch.Insert(newEvents[i]);
            }
            await table.ExecuteBatchAsync(batch);
        }

        // Gets all entities in table with matching Partition key
        public async Task<TriviaQuestion[]> GetByPartitionKeyTrivia(string tableName, string partitionKey)
        {
            CloudTable table = TableClient.GetTableReference(tableName);
            TableQuery<TriviaQuestion> query = new TableQuery<TriviaQuestion>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            return table.ExecuteQuery(query).ToArray();
        }

        // Gets all entities in table with matching Partition key
        public async Task<CalendarEvent[]> GetByPartitionKey(string tableName, string partitionKey)
        {
            CloudTable table = TableClient.GetTableReference(tableName);
            TableQuery<CalendarEvent> query = new TableQuery<CalendarEvent>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            return table.ExecuteQuery(query).ToArray();
        }

        // Gets all entities in table with a partition key that contains a specified string
        public async Task<CalendarEvent[]> GetByPartitionKeyContains(string tableName, string partitionKeyStart)
        {
            CloudTable table = TableClient.GetTableReference(tableName);
            TableQuery<CalendarEvent> query = new TableQuery<CalendarEvent>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, partitionKeyStart));
            return table.ExecuteQuery(query).ToArray();
        }

        // Deletes a specific entity from a table
        public async Task DeleteEvent(string tableName, CalendarEvent oldEvent)
        {
            try
            {
                CloudTable table = TableClient.GetTableReference(tableName);
                TableOperation operation = TableOperation.Retrieve<CalendarEvent>(oldEvent.PartitionKey, oldEvent.RowKey);
                TableResult result = table.Execute(operation);
                CalendarEvent entity = (CalendarEvent)result.Result;
                if (entity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(oldEvent);
                    await table.ExecuteAsync(deleteOperation);
                }
            }
            catch(System.Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        // Deletes a specific Question from the table
        public async Task DeleteQuestion(string tableName, TriviaQuestion old)
        {
            try
            {
                CloudTable table = TableClient.GetTableReference(tableName);
                TableOperation operation = TableOperation.Retrieve<TriviaQuestion>(old.PartitionKey, old.RowKey);
                TableResult result = table.Execute(operation);
                TriviaQuestion entity = (TriviaQuestion)result.Result;
                if (entity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(old);
                    await table.ExecuteAsync(deleteOperation);
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        // Replace an entity in a table with another entity
        public async Task ReplaceEvent(string tableName, CalendarEvent oldEvent, CalendarEvent newEvent)
        {
            CloudTable table = TableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Retrieve<CalendarEvent>(oldEvent.PartitionKey, oldEvent.RowKey);
            TableResult result = table.Execute(operation);
            CalendarEvent entity = (CalendarEvent)result.Result;
            if (entity != null)
            {
                if (oldEvent.PartitionKey == newEvent.PartitionKey && oldEvent.RowKey == newEvent.RowKey)
                {
                    TableOperation updateOperation = TableOperation.Replace(newEvent);
                    await table.ExecuteAsync(updateOperation);
                }
                else
                {
                    await DeleteEvent(tableName, oldEvent);
                    await AddEvent(tableName, newEvent);
                }
            }
            else
            {
                await AddEvent(tableName, newEvent);
            }
        }

        // Replace an entity in a table with another entity
        public async Task ReplaceQuestion(string tableName, TriviaQuestion oldQuestion, TriviaQuestion newQuestion)
        {
            CloudTable table = TableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Retrieve<TriviaQuestion>(oldQuestion.PartitionKey, oldQuestion.RowKey);
            TableResult result = table.Execute(operation);
            TriviaQuestion entity = (TriviaQuestion)result.Result;
            if (entity != null)
            {
                if (oldQuestion.PartitionKey == newQuestion.PartitionKey && oldQuestion.RowKey == newQuestion.RowKey)
                {
                    TableOperation updateOperation = TableOperation.Replace(newQuestion);
                    await table.ExecuteAsync(updateOperation);
                }
                else
                {
                    await DeleteQuestion(tableName, oldQuestion);
                    await AddQuestion(tableName, newQuestion);
                }
            }
            else
            {
                await AddQuestion(tableName, newQuestion);
            }
        }

        #endregion
    }
}
