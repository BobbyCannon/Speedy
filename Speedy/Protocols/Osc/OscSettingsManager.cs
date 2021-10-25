#region References

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	public class OscSettingsManager : IEnumerable<OscSetting>
	{
		#region Fields

		private readonly ConcurrentDictionary<string, OscSetting> _settings;

		#endregion

		#region Constructors

		public OscSettingsManager(params OscSetting[] settings)
		{
			_settings = new ConcurrentDictionary<string, OscSetting>();

			AddRange(settings);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Add a setting to the manager.
		/// </summary>
		/// <param name="setting"> The setting to add. </param>
		public T Add<T>(T setting) where T : OscSetting
		{
			_settings.AddOrUpdate(setting.Address, x => setting, (s, os) => setting);
			OnSettingAdded(setting);
			return setting;
		}

		/// <summary>
		/// Add a collection of settings to the manager.
		/// </summary>
		/// <param name="settings"> The settings to add. </param>
		public void AddRange<T>(IEnumerable<T> settings) where T : OscSetting
		{
			foreach (var s in settings)
			{
				Add(s);
			}
		}

		/// <summary>
		/// Remove all settings from the manager.
		/// </summary>
		public void Clear()
		{
			foreach (var s in this)
			{
				Remove(s);
			}
		}

		/// <inheritdoc />
		public IEnumerator<OscSetting> GetEnumerator()
		{
			return _settings.Values.GetEnumerator();
		}

		/// <summary>
		/// Remove a setting from the manager.
		/// </summary>
		/// <param name="setting"> The setting to be removed. </param>
		public void Remove<T>(T setting) where T : OscSetting
		{
			if (_settings.TryRemove(setting.Address, out var s))
			{
				OnSettingRemoved(s);
			}
		}

		/// <summary>
		/// Invokes a read request to all settings.
		/// </summary>
		public void RequestAllSettings()
		{
			foreach (var s in this.ToList())
			{
				if (s.CanRead)
				{
					s.RequestRead();
				}
			}
		}

		/// <summary>
		/// Invokes a read request to all changed or unread settings.
		/// </summary>
		public void RequestChangedSettings()
		{
			foreach (var s in this)
			{
				if (s.CanRead && (s.HasBeenUpdated || !s.HasBeenRead))
				{
					s.RequestRead();
				}
			}
		}

		/// <summary>
		/// Invokes a read request to all unread settings.
		/// </summary>
		public void RequestUnreadSettings()
		{
			foreach (var s in this)
			{
				if (s.CanRead && !s.HasBeenRead)
				{
					s.RequestRead();
				}
			}
		}

		/// <summary>
		/// Reset the read status. This allows us to then request all settings to refresh all settings.
		/// </summary>
		public void ResetAllReadStatus()
		{
			foreach (var s in this)
			{
				if (s.CanRead)
				{
					s.HasBeenRead = false;
				}
			}
		}

		/// <summary>
		/// Sends all setting to the server. The setting must have been read at
		/// least once. All non read settings will be ignored.
		/// </summary>
		/// <param name="force"> Force the setting to send even if never read. Default to false forcing the value to be read before it can be updated. </param>
		public void SendAllSettings(bool force = false)
		{
			foreach (var s in this)
			{
				if (s.CanWrite && (s.HasBeenRead || force))
				{
					s.RequestWrite();
				}
			}
		}

		/// <summary>
		/// Sends all settings that have been changed to the server.
		/// </summary>
		/// <param name="force">
		/// Force the settings to send even if never read. Default to false forcing the values to be read before they can be updated.
		/// </param>
		public void SendChangedSettings(bool force = false)
		{
			foreach (var s in this)
			{
				if (s.CanWrite && (s.HasBeenRead && s.HasBeenUpdated || force))
				{
					s.RequestWrite();
				}
			}
		}

		/// <summary>
		/// Reset all settings that have been changed back to the values in the original message.
		/// </summary>
		/// <param name="force">
		/// Force the settings to send even if never read. Default to false forcing the values to be read before they can be updated.
		/// </param>
		public void UndoChangedSettings(bool force = false)
		{
			foreach (var s in this)
			{
				if (s.HasBeenRead && s.HasBeenUpdated || force)
				{
					s.UndoChanges();
				}
			}
		}

		/// <summary>
		/// Update a manager setting with a new value.
		/// </summary>
		/// <param name="message"> The value to update with </param>
		public void UpdateSetting(OscMessage message)
		{
			foreach (var s in this)
			{
				if (s.Address != message.Address)
				{
					continue;
				}

				s.Update(message);
				OnSettingUpdated(s);
			}
		}

		/// <summary>
		/// Update a manager setting with a new value.
		/// </summary>
		/// <param name="setting"> The value to update with. </param>
		public void UpdateSetting(OscSetting setting)
		{
			foreach (var s in this)
			{
				if (s.Address != setting.Address)
				{
					continue;
				}

				s.Update(setting);
				OnSettingUpdated(s);
			}
		}

		protected virtual void OnSettingAdded(OscSetting e)
		{
			SettingAdded?.Invoke(this, e);
		}

		protected virtual void OnSettingRemoved(OscSetting e)
		{
			SettingRemoved?.Invoke(this, e);
		}

		protected virtual void OnSettingUpdated(OscSetting e)
		{
			SettingUpdated?.Invoke(this, e);
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Events

		public event EventHandler<OscSetting> SettingAdded;
		public event EventHandler<OscSetting> SettingRemoved;
		public event EventHandler<OscSetting> SettingUpdated;

		#endregion
	}
}