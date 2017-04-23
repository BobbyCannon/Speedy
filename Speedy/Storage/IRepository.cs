#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy.Storage
{
	internal interface IRepository : IDisposable
	{
		#region Methods

		void AssignKey(IEntity item, List<IEntity> processed);

		void AssignKeys(List<IEntity> processed);

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