#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Profiling;

/// <summary>
/// Represents a tracker path.
/// </summary>
public class TrackerPath : IDisposable
{
	#region Constructors

	/// <summary>
	/// Instantiates a new instance of the class.
	/// </summary>
	public TrackerPath()
	{
		var currentTime = TimeService.UtcNow;
		Children = new List<TrackerPath>();
		CompletedOn = currentTime;
		Id = Guid.NewGuid();
		Name = string.Empty;
		StartedOn = currentTime;
		Values = new List<TrackerPathValue>();
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets or set the child paths.
	/// </summary>
	public ICollection<TrackerPath> Children { get; set; }

	/// <summary>
	/// Gets or set the date and time the path was completed.
	/// </summary>
	public DateTime CompletedOn { get; set; }

	/// <summary>
	/// Gets or sets the data.
	/// </summary>
	public string Data { get; set; }

	/// <summary>
	/// Gets or sets the elapsed time between the started on and completed on.
	/// </summary>
	public TimeSpan ElapsedTime => CompletedOn - StartedOn;

	/// <summary>
	/// Gets or sets the unique ID.
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Returns true if the path has been completed.
	/// </summary>
	public bool IsCompleted { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the parent ID.
	/// </summary>
	public Guid ParentId { get; set; }

	/// <summary>
	/// Gets or set the date and time the path was started.
	/// </summary>
	public DateTime StartedOn { get; set; }

	/// <summary>
	/// Gets or sets the path type.
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	/// Gets or sets the values.
	/// </summary>
	public IList<TrackerPathValue> Values { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Completes the path and adds it to the path or tracker.
	/// </summary>
	public TrackerPath Abort()
	{
		IsCompleted = true;
		return this;
	}

	/// <summary>
	/// Adds an exception to this path.
	/// </summary>
	/// <param name="exception"> The exception to be added to the path. </param>
	/// <param name="values"> Optional values for this exception. </param>
	public void AddException(Exception exception, params TrackerPathValue[] values)
	{
		Children.Add(CreatePath(Id, exception, values));
	}

	/// <summary>
	/// Adds a child path to this path.
	/// </summary>
	/// <param name="name"> The name of the path. </param>
	/// <param name="values"> Optional values for this path. </param>
	public void AddPath(string name, params TrackerPathValue[] values)
	{
		Children.Add(new TrackerPath { ParentId = Id, Name = name, Values = values.ToList() });
	}

	/// <summary>
	/// Adds a value to this path.
	/// </summary>
	/// <param name="name"> The name of this value. </param>
	/// <param name="value"> The value of this value. </param>
	public void AddValue(string name, string value)
	{
		Values.AddOrUpdate(new TrackerPathValue { Name = name, Value = value });
	}

	/// <summary>
	/// Completes the path and adds it to the path or tracker.
	/// </summary>
	public TrackerPath Complete()
	{
		IsCompleted = true;
		CompletedOn = TimeService.UtcNow;
		Completed?.Invoke(this);
		return this;
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
	/// Starts a new path. Once the path is done be sure to call <seealso cref="Complete" />.
	/// </summary>
	/// <param name="name"> The name of the path. </param>
	/// <param name="values"> Optional values for this path. </param>
	/// <returns> The path for tracking a path. </returns>
	public TrackerPath StartNewPath(Func<string> name, params TrackerPathValue[] values)
	{
		var response = new TrackerPath { ParentId = Id, Name = name(), Values = values.ToList() };
		response.Completed += ResponseOnCompleted;
		response.Disposed += ResponseOnDisposed;
		return response;
	}

	/// <summary>
	/// Process an action and then add the path.
	/// </summary>
	/// <param name="key"> The key for the path. </param>
	/// <param name="action"> The action to process. </param>
	public void TrackAction(Func<string> key, Action<TrackerPath> action)
	{
		if (IsCompleted)
		{
			action(this);
			return;
		}

		using var result = new TrackerPath { Name = key(), StartedOn = TimeService.UtcNow };
		action(result);
		Children.Add(result);
	}

	/// <summary>
	/// Process an action and then add the path.
	/// </summary>
	/// <typeparam name="T"> The type of the response for the action. </typeparam>
	/// <param name="key"> The key for the path. </param>
	/// <param name="action"> The action to process. </param>
	/// <returns> The result of the action. </returns>
	public T TrackAction<T>(Func<string> key, Func<TrackerPath, T> action)
	{
		if (IsCompleted)
		{
			return action(this);
		}

		using var result = new TrackerPath { Name = key(), StartedOn = TimeService.UtcNow };
		var response = action(result);
		Children.Add(result);
		return response;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <param name="disposing"> Should be true if managed resources should be disposed. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}

		if (!IsCompleted)
		{
			Complete();
		}

		Disposed?.Invoke(this);
	}

	/// <summary>
	/// Starts a new path including an exception. The path will need to be completed or disposed before it will be added to the tracker.
	/// </summary>
	/// <param name="parentId"> The ID of the parent for this path. </param>
	/// <param name="ex"> The exception to be turned into a path. </param>
	/// <param name="values"> Optional values for this path. </param>
	/// <returns> The path for tracking a path. </returns>
	internal static TrackerPath CreatePath(Guid parentId, Exception ex, params TrackerPathValue[] values)
	{
		if (ex == null)
		{
			throw new ArgumentNullException(nameof(ex), "The exception cannot be null.");
		}

		var pathValues = new List<TrackerPathValue>(values);
		pathValues.AddOrUpdate(new TrackerPathValue("Message", ex.Message), new TrackerPathValue("Stack Trace", ex.StackTrace ?? string.Empty));

		var response = new TrackerPath
		{
			ParentId = parentId,
			Name = ex.GetType().Name,
			Values = pathValues.ToList(),
			Type = "Exception"
		};

		if (ex.InnerException == null)
		{
			return response;
		}

		var childException = CreatePath(parentId, ex.InnerException);
		response.Children.Add(childException);
		return response;
	}

	private void ResponseOnCompleted(TrackerPath path)
	{
		Children.Add(path);
	}

	private void ResponseOnDisposed(TrackerPath path)
	{
		path.Completed -= ResponseOnCompleted;
		path.Disposed -= ResponseOnDisposed;
	}

	#endregion

	#region Events

	/// <summary>
	/// Occurs when the path is completed.
	/// </summary>
	internal event Action<TrackerPath> Completed;

	/// <summary>
	/// Occurs when the path is disposed.
	/// </summary>
	internal event Action<TrackerPath> Disposed;

	#endregion
}