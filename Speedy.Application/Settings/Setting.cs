#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Sync;

#endregion

namespace Speedy.Application.Settings;

/// <summary>
/// Represents a setting.
/// </summary>
/// <typeparam name="T"> The type of the Value of the setting. </typeparam>
/// <typeparam name="T2"> The type of the Id of the setting. </typeparam>
public class Setting<T, T2> : Setting<T2>
{
	#region Constructors

	/// <summary>
	/// Represents a setting.
	/// </summary>
	public Setting()
	{
	}

	/// <summary>
	/// Represents a setting.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public Setting(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// The typed data of the value property.
	/// </summary>
	public T Data { get; set; }

	/// <inheritdoc />
	public override string Value
	{
		get => Data.ToRawJson();
		set => Data = value.FromJson<T>();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override bool HasChanges(params string[] exclusions)
	{
		if (Data is IChangeable changeable && changeable.HasChanges())
		{
			return true;
		}

		return base.HasChanges(exclusions);
	}

	/// <inheritdoc />
	public override void ResetHasChanges()
	{
		if (Data is IChangeable changeable)
		{
			changeable.ResetHasChanges();
		}

		base.ResetHasChanges();
	}

	/// <summary>
	/// Update the Setting`2 with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public bool UpdateWith(Setting<T, T2> update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return false;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			Data = update.Data;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Data)), x => x.Data = update.Data);
		}

		var set = new HashSet<string>(exclusions) { nameof(Value) };
		return base.UpdateWith(update, set.ToArray());
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			Setting<T, T2> value => UpdateWith(value, exclusions),
			Setting<T2> value => UpdateWith(value, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	#endregion
}

/// <summary>
/// Represents a setting.
/// </summary>
/// <typeparam name="T"> The type of the Value of the setting. </typeparam>
public class Setting<T> : SyncModel<T>
{
	#region Constructors

	/// <summary>
	/// Represents a setting.
	/// </summary>
	public Setting()
	{
	}

	/// <summary>
	/// Represents a setting.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public Setting(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// Set to mark this setting as a syncable setting.
	/// </summary>
	public bool CanSync { get; set; }

	/// <summary>
	/// The category for the settings.
	/// </summary>
	public string Category { get; set; }

	/// <summary>
	/// Optionally expires on value, DateTime.MinValue means there is no expiration.
	/// </summary>
	public DateTime ExpiresOn { get; set; }

	/// <inheritdoc />
	public override T Id { get; set; }

	/// <summary>
	/// The name of the setting.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// The value of the setting in JSON format.
	/// </summary>
	public virtual string Value { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Update the Setting`1 with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public bool UpdateWith(Setting<T> update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return false;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			CanSync = update.CanSync;
			Category = update.Category;
			CreatedOn = update.CreatedOn;
			ExpiresOn = update.ExpiresOn;
			Id = update.Id;
			IsDeleted = update.IsDeleted;
			ModifiedOn = update.ModifiedOn;
			Name = update.Name;
			SyncId = update.SyncId;
			Value = update.Value;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(CanSync)), x => x.CanSync = update.CanSync);
			this.IfThen(_ => !exclusions.Contains(nameof(Category)), x => x.Category = update.Category);
			this.IfThen(_ => !exclusions.Contains(nameof(CreatedOn)), x => x.CreatedOn = update.CreatedOn);
			this.IfThen(_ => !exclusions.Contains(nameof(ExpiresOn)), x => x.ExpiresOn = update.ExpiresOn);
			this.IfThen(_ => !exclusions.Contains(nameof(Id)), x => x.Id = update.Id);
			this.IfThen(_ => !exclusions.Contains(nameof(IsDeleted)), x => x.IsDeleted = update.IsDeleted);
			this.IfThen(_ => !exclusions.Contains(nameof(ModifiedOn)), x => x.ModifiedOn = update.ModifiedOn);
			this.IfThen(_ => !exclusions.Contains(nameof(Name)), x => x.Name = update.Name);
			this.IfThen(_ => !exclusions.Contains(nameof(SyncId)), x => x.SyncId = update.SyncId);
			this.IfThen(_ => !exclusions.Contains(nameof(Value)), x => x.Value = update.Value);
		}

		return true;
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			Setting<T> value => UpdateWith(value, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	#endregion
}