#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace Speedy.Storage
{
	internal class EntityStore<T> : IEnumerable<T> where T : Entity, new()

	{
		#region Fields

		private readonly EntityRepository<T> _repository;

		private readonly JsonSerializer _serializer;

		#endregion

		#region Constructors

		public EntityStore(string directory, EntityRepository<T> repository)
		{
			Directory = directory;

			_repository = repository;
			_serializer = new JsonSerializer();
			_serializer.ContractResolver = new ShouldSerializeContractResolver();
		}

		#endregion

		#region Properties

		public string Directory { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			var enumerator = new EntityStoreEnumerator<T>(this, _repository);
			enumerator.UpdateEntityRelationships += OnUpdateEntityRelationships;
			return enumerator;
		}

		/// <summary>
		/// Removes the entity from the path.
		/// </summary>
		/// <param name="id"> The ID of the entity to remove. </param>
		public void Remove(int id)
		{
			var directory = new DirectoryInfo(Directory);
			directory.SafeCreate();

			var file = new FileInfo($"{Directory}\\{id}.json");
			file.SafeDelete();
		}

		/// <summary>
		/// Writes the entity to the path.
		/// </summary>
		/// <param name="entity"> The entity to write. </param>
		public void Write(Entity entity)
		{
			var directory = new DirectoryInfo(Directory);
			directory.SafeCreate();

			var filePath = $"{Directory}\\{entity.Id}.json";

			using (var writer = new StreamWriter(new FileStream(filePath, FileMode.Create), Encoding.UTF8))
			{
				_serializer.Serialize(writer, entity);
			}
		}

		protected virtual void OnUpdateEntityRelationships(T obj)
		{
			var handler = UpdateEntityRelationships;
			handler?.Invoke(obj);
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Events

		public event Action<T> UpdateEntityRelationships;

		#endregion
	}
}