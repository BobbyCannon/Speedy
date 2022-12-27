#region References

using System;

#endregion

namespace Speedy;

/// <summary>
/// The session for storing content into a database.
/// </summary>
/// <typeparam name="T"> The database type. </typeparam>
public class DatabaseSession<T> : IDatabaseSession<T> where T : IDatabase
{
	#region Fields

	private int _count;
	private T _currentDatabase;
	private readonly IDatabaseProvider<T> _databaseProvider;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an session for a database.
	/// </summary>
	/// <param name="databaseProvider"> The database provider. </param>
	public DatabaseSession(IDatabaseProvider<T> databaseProvider)
	{
		_databaseProvider = databaseProvider;
		_count = 0;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Add the entity to the database session.
	/// </summary>
	/// <typeparam name="TEntity"> The type of the entity. </typeparam>
	/// <typeparam name="TPrimaryKey"> The type of the primary key. </typeparam>
	/// <param name="entity"> The entity to add. </param>
	/// <returns> The added entity. </returns>
	public TEntity Add<TEntity, TPrimaryKey>(TEntity entity) where TEntity : Entity<TPrimaryKey>
	{
		_currentDatabase ??= _databaseProvider.GetDatabase();
		var repository = _currentDatabase.GetRepository<TEntity, TPrimaryKey>();
		repository.Add(entity);

		_count += _currentDatabase.SaveChanges();

		if (_count >= 600)
		{
			_currentDatabase?.Dispose();
			_currentDatabase = default;
			_count = 0;
		}

		return entity;
	}

	/// <summary>
	/// Add the entity to the database session.
	/// </summary>
	/// <typeparam name="TEntity"> The type of the entity. </typeparam>
	/// <typeparam name="TPrimaryKey"> The type of the primary key. </typeparam>
	/// <param name="update"> The action to update the entity. </param>
	/// <returns> The added entity. </returns>
	public TEntity Add<TEntity, TPrimaryKey>(Action<TEntity> update)
		where TEntity : Entity<TPrimaryKey>, new()
	{
		_currentDatabase ??= _databaseProvider.GetDatabase();
		var repository = _currentDatabase.GetRepository<TEntity, TPrimaryKey>();
		var entity = new TEntity();
		update(entity);
		repository.Add(entity);

		_count += _currentDatabase.SaveChanges();

		if (_count >= 600)
		{
			_currentDatabase?.Dispose();
			_currentDatabase = default;
			_count = 0;
		}

		return entity;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <param name="disposing"> True if disposing and false if otherwise. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}

		_currentDatabase?.SaveChanges();
		_currentDatabase?.Dispose();
	}

	#endregion
}

/// <summary>
/// The session for storing content into a database.
/// </summary>
/// <typeparam name="T"> The database type. </typeparam>
public interface IDatabaseSession<out T> : IDisposable where T : IDatabase
{
	#region Methods

	/// <summary>
	/// Add the entity to the database session.
	/// </summary>
	/// <typeparam name="TEntity"> The type of the entity. </typeparam>
	/// <typeparam name="TPrimaryKey"> The type of the primary key. </typeparam>
	/// <param name="entity"> The entity to add. </param>
	/// <returns> The added entity. </returns>
	TEntity Add<TEntity, TPrimaryKey>(TEntity entity) where TEntity : Entity<TPrimaryKey>;

	/// <summary>
	/// Add the entity to the database session.
	/// </summary>
	/// <typeparam name="TEntity"> The type of the entity. </typeparam>
	/// <typeparam name="TPrimaryKey"> The type of the primary key. </typeparam>
	/// <param name="update"> The action to update the entity. </param>
	/// <returns> The added entity. </returns>
	TEntity Add<TEntity, TPrimaryKey>(Action<TEntity> update)
		where TEntity : Entity<TPrimaryKey>, new();

	#endregion
}