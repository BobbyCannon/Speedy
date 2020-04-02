#region References

using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace Speedy.Mobile.Services
{
	public interface IDataStore<T>
	{
		#region Methods

		Task<bool> AddItemAsync(T item);
		Task<bool> DeleteItemAsync(long id);
		Task<T> GetItemAsync(long id);
		Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
		Task<bool> UpdateItemAsync(T item);

		#endregion
	}
}