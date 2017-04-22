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

		bool HasDependentRelationship(object[] value, object id);

		object Read(object id);

		int SaveChanges();

		void UpdateRelationships();

		void ValidateEntities();

		#endregion
	}
}