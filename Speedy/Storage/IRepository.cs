#region References

using System;

#endregion

namespace Speedy.Storage
{
	internal interface IRepository : IDisposable
	{
		#region Methods

		void AssignKeys();

		bool HasChanges();

		Entity Read(int id);

		int SaveChanges();

		void UpdateLocalSyncIds();

		void UpdateRelationships();

		void ValidateEntities();

		bool HasDependentRelationship(object[] value, int id);

		#endregion
	}
}