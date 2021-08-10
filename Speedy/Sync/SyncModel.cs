#region References

using System.ComponentModel;
using Newtonsoft.Json;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync model, usually used in a web API model.
	/// </summary>
	/// <typeparam name="T"> The type for the key. </typeparam>
	public abstract class SyncModel<T> : SyncEntity<T>, IBindable
	{
		#region Constructors

		/// <summary>
		/// Instantiate a sync model.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		protected SyncModel(IDispatcher dispatcher = null)
		{
			Dispatcher = dispatcher;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Represents a thread dispatcher to help with cross threaded request.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		protected IDispatcher Dispatcher { get; private set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public IDispatcher GetDispatcher()
		{
			return Dispatcher;
		}

		/// <summary>
		/// Indicates the property has changed on the bindable object.
		/// </summary>
		/// <param name="propertyName"> The name of the property has changed. </param>
		public override void OnPropertyChanged(string propertyName)
		{
			if (Dispatcher != null && !Dispatcher.HasThreadAccess)
			{
				Dispatcher.Run(() => OnPropertyChanged(propertyName));
				return;
			}

			base.OnPropertyChanged(propertyName);
		}

		/// <inheritdoc />
		public virtual void UpdateDispatcher(IDispatcher dispatcher)
		{
			Dispatcher = dispatcher;
		}

		#endregion
	}
}