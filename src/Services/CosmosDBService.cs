using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RSSProject.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
namespace RSSProject.Services
{
    

    public class CosmosDbService : ICosmosDbService
    {
        private Container _container;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(RSS item)
        {
            await this._container.CreateItemAsync<RSS>(item, new PartitionKey(item.Id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await this._container.DeleteItemAsync<RSS>(id, new PartitionKey(id));
        }

        public async Task<RSS> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<RSS> response = await this._container.ReadItemAsync<RSS>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

        }

        public async Task<IEnumerable<RSS>> GetItemsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<RSS>(new QueryDefinition(queryString));
            List<RSS> results = new List<RSS>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync(string id, RSS item)
        {
            await this._container.UpsertItemAsync<RSS>(item, new PartitionKey(id));
        }
    }
}
