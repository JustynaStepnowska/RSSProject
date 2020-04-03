
using System.Collections.Generic;
using System.Threading.Tasks;
using RSSProject.Models;

namespace RSSProject.Services
{
    public interface ICosmosDbService
    {
        Task<IEnumerable<RSS>> GetItemsAsync(string query);
        Task<RSS> GetItemAsync(string id);
        Task AddItemAsync(RSS item);
        Task UpdateItemAsync(string id, RSS item);
        Task DeleteItemAsync(string id);
    }
}
