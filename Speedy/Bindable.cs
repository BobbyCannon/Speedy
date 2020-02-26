#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a bindable object.
	/// </summary>
	public abstract class Bindable<T> : Bindable, IUpdatable<T>
	{
		#region Constructors

		/// <summary>
		/// Instantiates a bindable object.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		protected Bindable(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Allows updating of one type to another based on member Name and Type.
		/// </summary>
		/// <param name="update"> The source of the updates. </param>
		/// <param name="excludeVirtuals"> An optional value to exclude virtual members. Defaults to true. </param>
		/// <param name="exclusions"> An optional list of members to exclude. </param>
		public virtual void UpdateWith(T update, bool excludeVirtuals, params string[] exclusions)
		{
			var totalExclusions = new HashSet<string>(exclusions);
			if (excludeVirtuals)
			{
				totalExclusions.AddRange(RealType.GetVirtualPropertyNames());
			}

			UpdateWith(update, totalExclusions.ToArray());
		}

		/// <inheritdoc />
		public virtual void UpdateWith(T update, params string[] exclusions)
		{
			UpdatableExtensions.UpdateWith(this, update, exclusions);
		}

		/// <inheritdoc />
		public virtual void UpdateWithOnly(T update, params string[] inclusions)
		{
			UpdatableExtensions.UpdateWithOnly(this, update, inclusions);
		}

		#endregion
	}

	/// <summary>
	/// Represents a bindable object.
	/// </summary>
	public abstract class Bindable : IUpdatable, INotifyPropertyChanged
	{
		#region Fields

		private Type _realType;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a bindable object.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		protected Bindable(IDispatcher dispatcher = null)
		{
			Dispatcher = dispatcher;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Determines if the object has changes.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public virtual bool HasChanges { get; set; }

		/// <summary>
		/// Represents a thread dispatcher to help with cross threaded request.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		protected IDispatcher Dispatcher { get; private set; }

		/// <summary>
		/// Cached version of the "real" type, meaning not EF proxy but rather root type
		/// </summary>
		internal Type RealType => _realType ??= this.GetRealType();

		#endregion

		#region Methods

		/// <summary>
		/// Indicates the property has changed on the bindable object.
		/// </summary>
		/// <param name="propertyName"> The name of the property has changed. </param>
		public virtual void OnPropertyChanged(string propertyName = null)
		{
			// todo: move this to another virtual method that would then be called by this on property changed
			// I had to move this dispatcher code up. Also would be nice to only dispatch specific properties, right?

			if (Dispatcher != null && !Dispatcher.HasThreadAccess)
			{
				Dispatcher.Run(() => OnPropertyChanged(propertyName));
				return;
			}

			if (propertyName != nameof(HasChanges))
			{
				HasChanges = true;
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Updates the entity for this entity.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		public virtual void UpdateDispatcher(IDispatcher dispatcher)
		{
			Dispatcher = dispatcher;
		}

		/// <summary>
		/// Allows updating of one type to another based on member Name and Type.
		/// </summary>
		/// <param name="update"> The source of the updates. </param>
		/// <param name="excludeVirtuals"> An optional value to exclude virtual members. Defaults to true. </param>
		/// <param name="exclusions"> An optional list of members to exclude. </param>
		public virtual void UpdateWith(object update, bool excludeVirtuals, params string[] exclusions)
		{
			var totalExclusions = new HashSet<string>(exclusions);
			if (excludeVirtuals)
			{
				totalExclusions.AddRange(RealType.GetVirtualPropertyNames());
			}

			UpdateWith(update, totalExclusions.ToArray());
		}

		/// <inheritdoc />
		public virtual void UpdateWith(object update, params string[] exclusions)
		{
			UpdatableExtensions.UpdateWith(this, update, exclusions);
		}

		/// <inheritdoc />
		public virtual void UpdateWithOnly(object update, params string[] inclusions)
		{
			UpdatableExtensions.UpdateWithOnly(this, update, inclusions);
		}

		#endregion

		#region Events

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}