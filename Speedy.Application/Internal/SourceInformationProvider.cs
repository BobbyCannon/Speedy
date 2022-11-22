#region References

using Speedy.Devices;

#endregion

namespace Speedy.Application.Internal;

/// <summary>
/// The source of a location provider.
/// </summary>
internal class SourceInformationProvider : Bindable, IInformationProvider
{
	#region Constructors

	/// <summary>
	/// Instantiate an instance of a location provider source.
	/// </summary>
	public SourceInformationProvider() : this(null)
	{
	}

	/// <summary>
	/// Instantiate an instance of a location provider source.
	/// </summary>
	public SourceInformationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public bool IsEnabled { get; set; }

	/// <inheritdoc />
	public bool IsMonitoring { get; set; }

	/// <inheritdoc />
	public string ProviderName { get; set; }

	#endregion
}