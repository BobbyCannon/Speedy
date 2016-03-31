#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			return new EntityStoreEnumerator<T>(this);
		}

		/// <summary>
		/// Get entity by ID.
		/// </summary>
		/// <param name="id"> The ID of the entity to read. </param>
		/// <returns> The entity or null. </returns>
		public Entity Read(int? id)
		{
			if (!id.HasValue)
			{
				return null;
			}

			var file = new FileInfo($"{Directory}\\{id}.json");
			return file.Exists ? ReadEntity(file.FullName) : null;
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
		/// Read the entity from the file path provided.
		/// </summary>
		/// <param name="filePath"> The file path for the entity. </param>
		/// <returns> The entity that was read or null otherwise. </returns>
		internal Entity ReadEntity(string filePath)
		{
			using (var reader = new JsonTextReader(new StreamReader(filePath, Encoding.UTF8)))
			{
				var readEntity = _serializer.Deserialize<T>(reader);
				var existing = _repository.Cache.FirstOrDefault(x => x.Entity.Id == readEntity.Id || x.OldEntity.Id == readEntity.Id);
				if (existing != null)
				{
					return (T) existing.Entity;
				}

				_repository.AddOrUpdate(readEntity);
				OnUpdateEntityRelationships(readEntity);
				return readEntity;
			}
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