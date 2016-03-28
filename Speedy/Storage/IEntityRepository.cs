namespace Speedy.Storage
{
	internal interface IEntityRepository
	{
		#region Methods

		void AssignKeys();

		void Reset();

		int SaveChanges();

		void UpdateRelationships();

		void ValidateEntities();

		#endregion
	}
}