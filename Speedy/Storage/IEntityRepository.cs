namespace Speedy.Storage
{
	internal interface IEntityRepository
	{
		#region Methods

		void AssignKeys();

		Entity GetEntity(int? id);

		void Reset();

		int SaveChanges();

		void UpdateRelationships();

		void ValidateEntities();

		#endregion
	}
}