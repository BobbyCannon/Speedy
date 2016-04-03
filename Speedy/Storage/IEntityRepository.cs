#region References

using System;

#endregion

namespace Speedy.Storage
{
	internal interface IEntityRepository : IDisposable
	{
		#region Methods

		void AssignKeys();

		Entity Read(int id);

		int SaveChanges();

		void UpdateRelationships();

		void ValidateEntities();

		#endregion
	}
}