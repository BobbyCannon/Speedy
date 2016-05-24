#region References

using System;

#endregion

namespace Speedy.Storage
{
	internal interface IRepository : IDisposable
	{
		#region Methods

		void AssignKeys();

		int DiscardChanges();

		bool HasChanges();

		bool HasDependentRelationship(object[] value, int id);

		Entity Read(int id);

		int SaveChanges();

		void UpdateLocalSyncIds();

		void UpdateRelationships();

		void ValidateEntities();

		#endregion
	}
}