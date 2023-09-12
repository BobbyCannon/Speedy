#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Application.Settings;

#endregion

namespace Speedy.Data.SyncApi;

/// <summary>
/// Represents the public setting model.
/// </summary>
public class Setting : Setting<long>
{
	#region Constructors

	[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
	public Setting()
	{
		ResetHasChanges();
	}

	#endregion

	#region Properties

	public override long Id { get; set; }

	#endregion
}