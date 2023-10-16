#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy.Application.Settings;

/// <summary>
/// Represents a manager for a category of settings.
/// </summary>
/// <typeparam name="T"> The type of the setting. </typeparam>
/// <typeparam name="T2"> The type of the setting ID. </typeparam>
public abstract class SettingsManager<T, T2> : Bindable
	where T : Setting<T2>, new()
{
	#region Fields

	private readonly string _category;
	private readonly string[] _settingExclusions;
	private readonly IDictionary<string, Setting<T2>> _settings;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an instance of the settings manager.
	/// </summary>
	/// <param name="category"> The category name. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected SettingsManager(string category, IDispatcher dispatcher) : base(dispatcher)
	{
		_category = category;
		_settings = new Dictionary<string, Setting<T2>>();
		_settingExclusions = new[] { "Id", "Name", "Category" };
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override bool HasChanges()
	{
		return _settings.Values.Any(x => x.HasChanges());
	}

	/// <summary>
	/// Load the settings from the repository.
	/// </summary>
	/// <param name="repository"> The repository contains the settings. </param>
	public void Load(IRepository<T, T2> repository)
	{
		foreach (var setting in _settings.Values)
		{
			var entity = repository.FirstOrDefault(x => (x.Name == setting.Name) && (x.Category == _category));
			if (entity == null)
			{
				continue;
			}

			setting.UpdateWith(entity, _settingExclusions);
		}

		ResetHasChanges();
	}

	/// <summary>
	/// Load the settings from the repository.
	/// </summary>
	/// <param name="repository"> The repository contains the settings. </param>
	public void Load(ICollection<T> repository)
	{
		foreach (var setting in _settings.Values)
		{
			var entity = repository.FirstOrDefault(x => (x.Name == setting.Name) && (x.Category == _category));
			if (entity == null)
			{
				continue;
			}

			setting.UpdateWith(entity, _settingExclusions);
		}

		ResetHasChanges();
	}

	/// <inheritdoc />
	public override void ResetHasChanges()
	{
		_settings.ForEach(x => x.Value.ResetHasChanges());
		base.ResetHasChanges();
	}

	/// <summary>
	/// Save the settings to the repository.
	/// </summary>
	/// <param name="repository"> The repository to store the settings. </param>
	public void Save(IRepository<T, T2> repository)
	{
		foreach (var setting in _settings.Values)
		{
			var entity = repository.FirstOrDefault(x => (x.Name == setting.Name) && (x.Category == _category));

			if (entity != null)
			{
				entity.UpdateWith(setting, _settingExclusions);
			}
			else
			{
				entity = new T { Name = setting.Name, Category = _category };
				entity.UpdateWith(setting, _settingExclusions);
				repository.Add(entity);
			}
		}

		ResetHasChanges();
	}

	/// <summary>
	/// Check to see if the setting should be a local only setting. Any local setting cannot be synced.
	/// </summary>
	/// <param name="name"> The setting name to be tested. </param>
	/// <returns> True if the setting is local otherwise false. </returns>
	protected virtual bool CanSync(string name)
	{
		return false;
	}

	/// <summary>
	/// Get the value of a setting.
	/// </summary>
	/// <typeparam name="TData"> The type of the Data value. </typeparam>
	/// <param name="name"> The name of the setting. </param>
	/// <param name="defaultValue"> The default value to return if not found. </param>
	/// <returns> The value read or the default value. </returns>
	protected TData Get<TData>(string name, TData defaultValue = default)
	{
		if (!_settings.TryGetValue(name, out var setting))
		{
			return InitializeSetting(name, defaultValue).Data;
		}

		if (setting is Setting<TData, T2> typedSetting)
		{
			return typedSetting.Data;
		}

		return setting.Value.FromJson<TData>();
	}

	/// <summary>
	/// Initialize a setting.
	/// </summary>
	/// <param name="name"> The name of the settings. </param>
	/// <param name="defaultValue"> The default value for the setting. </param>
	/// <param name="update"> An optional update action. </param>
	/// <typeparam name="TData"> The type of the data for the setting. </typeparam>
	/// <returns> The new setting. </returns>
	protected Setting<TData, T2> InitializeSetting<TData>(string name, TData defaultValue = default, Action<Setting<TData, T2>> update = null)
	{
		var response = new Setting<TData, T2>(GetDispatcher())
		{
			Name = name,
			Data = defaultValue
		};
		update?.Invoke(response);
		_settings.Add(name, response);
		return response;
	}

	/// <summary>
	/// Get the value of a setting.
	/// </summary>
	/// <typeparam name="TData"> The type of the Data value. </typeparam>
	/// <param name="name"> The name of the setting. </param>
	/// <param name="value"> The value to be set. </param>
	/// <returns> The value set. </returns>
	protected TData Set<TData>(string name, TData value)
	{
		if (!_settings.TryGetValue(name, out var setting))
		{
			return InitializeSetting(name, value).Data;
		}

		if (setting is Setting<TData, T2> typeSetting)
		{
			typeSetting.Data = value;
			return typeSetting.Data;
		}

		setting.Value = value.ToJson();
		return value;
	}

	#endregion
}