namespace Speedy.Website.Models
{
	public class CustomPagedResults<T> : PagedResults<T, CustomPagedRequest>
	{
		#region Constructors

		public CustomPagedResults(CustomPagedRequest request, int totalCount, params T[] results)
			: base(request, totalCount, results)
		{
		}

		#endregion
	}
}