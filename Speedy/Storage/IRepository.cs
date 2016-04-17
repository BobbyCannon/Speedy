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

		void UpdateRelationships();

		void ValidateEntities();

		#endregion
	}
}