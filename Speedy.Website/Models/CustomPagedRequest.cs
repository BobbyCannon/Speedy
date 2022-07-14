namespace Speedy.Website.Models
{
	public class CustomPagedRequest : PagedRequest
	{
		#region Properties

		public double Precision
		{
			get => Get(nameof(Precision), 0.0d);
			set => Set(nameof(Precision), value);
		}

		/// <inheritdoc />
		protected override int PerPageDefault => 11;

		/// <inheritdoc />
		protected override int PerPageMaxDefault => 100;

		#endregion
	}
}