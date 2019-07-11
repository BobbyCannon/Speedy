namespace Speedy.Storage
{
	internal class EntityState<T, T2> where T : Entity<T2>
	{
		#region Constructors

		public EntityState(T entity, T oldEntity, EntityStateType state)
		{
			Entity = entity;
			OldEntity = oldEntity;
			State = state;
		}

		#endregion

		#region Properties

		public T Entity { get; }
		public T OldEntity { get; }
		public EntityStateType State { get; set; }

		#endregion
	}
}