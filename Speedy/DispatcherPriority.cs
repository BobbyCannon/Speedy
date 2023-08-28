namespace Speedy;

/// <summary>
/// The priorities at which operations can be invoked via the Dispatcher.
/// </summary>
public enum DispatcherPriority
{
	/// <summary>
	/// Operations at this priority are processed at normal priority.
	/// </summary>
	Normal = 0,

	/// <summary>
	/// Operations at this priority are processed when the system is idle.
	/// </summary>
	SystemIdle = 1,

	/// <summary>
	/// Operations at this priority are processed when the application is idle.
	/// </summary>
	ApplicationIdle = 2,

	/// <summary>
	/// Operations at this priority are processed when the context is idle.
	/// </summary>
	ContextIdle = 3,

	/// <summary>
	/// Operations at this priority are processed after all other non-idle operations are done.
	/// </summary>
	Background = 4
}