namespace Speedy.Storage
{
	internal class EntityState<T, T2> where T : Entity<T2>
	{
		#region Properties

		public T Entity { get; set; }
		public T OldEntity { get; set; }
		public EntityStateType State { get; set; }

		#endregion
	}
}