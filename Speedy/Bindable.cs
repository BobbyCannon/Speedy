#region References

using System.ComponentModel;
using Newtonsoft.Json;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a bindable object.
	/// </summary>
	public abstract class Bindable : INotifyPropertyChanged
	{
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
		
		#endregion

		#region Events

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}