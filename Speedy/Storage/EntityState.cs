#region References

using System.ComponentModel;

#endregion

namespace Speedy.Storage
{
	internal class EntityState<T, T2> where T : Entity<T2>
	{
		#region Constructors

		public EntityState(T entity, T oldEntity, EntityStateType state)
		{
			Entity = entity;
			Entity.PropertyChanged += EntityOnPropertyChanged;
			OldEntity = oldEntity;
			State = state;
		}

		#endregion

		#region Properties

		public T Entity { get; }

		public T OldEntity { get; }

		public EntityStateType State { get; set; }

		#endregion

		#region Methods

		public void ResetChangeTracking()
		{
			Entity.ResetChangeTracking();
			OldEntity.ResetChangeTracking();
			State = EntityStateType.Unmodified;
		}

		public void ResetEvents()
		{
			Entity.PropertyChanged -= EntityOnPropertyChanged;
		}

		private void EntityOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (State == EntityStateType.Unmodified && Entity.HasChanges())
			{
				State = EntityStateType.Modified;
			}
		}

		#endregion
	}
}