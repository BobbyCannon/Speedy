#region References

using System;
using System.Drawing;
using System.Text;
using System.Threading;
using Speedy.Automation.Desktop;
using Speedy.Automation.Internal;

#endregion

namespace Speedy.Automation;

/// <summary>
/// Represents an element.
/// </summary>
public abstract class Element : ElementHost
{
	#region Constructors

	/// <summary>
	/// Instantiates an instance of an element.
	/// </summary>
	/// <param name="application"> The application this element exists in. </param>
	/// <param name="parent"> The parent of this element. </param>
	protected Element(Application application, ElementHost parent)
		: base(application, parent)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets the automation id of the element.
	/// </summary>
	public abstract string AutomationId { get; }

	/// <summary>
	/// Gets the coordinates of the rectangle that completely encloses the element.
	/// </summary>
	public Rectangle BoundingRectangle => new Rectangle(Location.X, Location.Y, Width, Height);

	/// <summary>
	/// Gets a value that indicates whether the element is enabled.
	/// </summary>
	public abstract bool Enabled { get; }

	/// <summary>
	/// Gets a value that indicates whether the element is focused.
	/// </summary>
	public abstract bool Focused { get; }

	/// <summary>
	/// Gets the full id of the element which include all parent IDs prefixed to this element ID.
	/// Includes full host namespace. Ex. GrandParent,Parent,Element
	/// </summary>
	public string FullId
	{
		get
		{
			var builder = new StringBuilder();
			var element = (ElementHost) this;

			do
			{
				builder.Insert(0, new[] { element.Id, element.Name, " " }.FirstValue() + ",");
				element = element.Parent;
			} while (element != null);

			builder.Remove(builder.Length - 1, 1);
			return builder.ToString();
		}
	}

	/// <summary>
	/// Gets the width of the element.
	/// </summary>
	public abstract int Height { get; }

	/// <summary>
	/// Gets or sets an attribute or property by name.
	/// </summary>
	/// <param name="id"> The ID of the attribute or property to read. </param>
	public abstract string this[string id] { get; set; }

	/// <summary>
	/// Gets the location of the element.
	/// </summary>
	public abstract Point Location { get; }

	/// <summary>
	/// Gets the size of the element.
	/// </summary>
	public Size Size => new Size(Width, Height);

	/// <summary>
	/// Gets the width of the element.
	/// </summary>
	public abstract int Width { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Performs mouse click at the center of the element.
	/// </summary>
	/// <param name="x"> Optional X offset when clicking. </param>
	/// <param name="y"> Optional Y offset when clicking. </param>
	/// <param name="refresh"> Optional value to refresh the element's children. </param>
	public abstract Element Click(int x = 0, int y = 0, bool refresh = true);

	/// <summary>
	/// Set focus on the element.
	/// </summary>
	public abstract Element Focus();

	/// <summary>
	/// Get next sibling. Returns null if no sibling is found.
	/// </summary>
	public Element GetNextSibling()
	{
		var index = Parent?.Children.IndexOf(this);
		if ((index == null) || ((index + 1) >= Parent?.Children.Count))
		{
			return null;
		}

		return Parent.Children[index.Value + 1];
	}

	/// <summary>
	/// Get previous sibling. Returns null if no sibling is found.
	/// </summary>
	public Element GetPreviousSibling()
	{
		var index = Parent?.Children.IndexOf(this);
		if ((index == null) || (index < 1))
		{
			return null;
		}

		return Parent.Children[index.Value - 1];
	}

	/// <summary>
	/// Performs mouse left click at the center of the element.
	/// </summary>
	/// <param name="x"> Optional X offset when clicking. </param>
	/// <param name="y"> Optional Y offset when clicking. </param>
	public abstract Element LeftClick(int x = 0, int y = 0);

	/// <summary>
	/// Performs mouse middle click at the center of the element.
	/// </summary>
	/// <param name="x"> Optional X offset when clicking. </param>
	/// <param name="y"> Optional Y offset when clicking. </param>
	public abstract Element MiddleClick(int x = 0, int y = 0);

	/// <summary>
	/// Moves mouse cursor to the center of the element.
	/// </summary>
	/// <param name="x"> Optional X offset when clicking. </param>
	/// <param name="y"> Optional Y offset when clicking. </param>
	public abstract Element MoveMouseTo(int x = 0, int y = 0);

	/// <summary>
	/// Performs mouse right click at the center of the element.
	/// </summary>
	/// <param name="x"> Optional X offset when clicking. </param>
	/// <param name="y"> Optional Y offset when clicking. </param>
	public abstract Element RightClick(int x = 0, int y = 0);

	/// <summary>
	/// Focus the element then sends provided text and an optional set of keys as input.
	/// </summary>
	/// <param name="text"> The value to type. </param>
	public virtual Element SendInput(string text)
	{
		return SendInput(text, TimeSpan.Zero);
	}

	/// <summary>
	/// Focus the element then sends provided text and an optional set of keys as input.
	/// </summary>
	/// <param name="text"> The value to type. </param>
	/// <param name="delay"> An optional delay after sending input. </param>
	public virtual Element SendInput(string text, int delay)
	{
		return SendInput(text, TimeSpan.FromMilliseconds(delay));
	}

	/// <summary>
	/// Focus the element then sends provided text and an optional set of keys as input.
	/// </summary>
	/// <param name="text"> The value to type. </param>
	/// <param name="delay"> An optional delay after sending input. </param>
	public virtual Element SendInput(string text, TimeSpan delay)
	{
		Application.BringToFront();
		Focus();
		Input.Keyboard.SendInput(text, delay);
		return this;
	}

	/// <summary>
	/// Focus the element then sends provided text and an optional set of keys as input.
	/// </summary>
	/// <param name="text"> The value to type. </param>
	/// <param name="keys"> An optional set of keyboard keys to press after typing the provided text. </param>
	public virtual Element SendInput(string text, params KeyboardKey[] keys)
	{
		Application.BringToFront();
		Focus();
		Input.Keyboard.SendInput(text, keys);
		return this;
	}

	/// <summary>
	/// Focus the element then sends provided text as input. Can delay with before sending an optional set of key strokes.
	/// </summary>
	/// <param name="text"> The value to type. </param>
	/// <param name="delay"> An optional delay before sending optional keys. </param>
	/// <param name="keys"> An optional set of keyboard keys to press after typing the provided text. </param>
	public virtual Element SendInput(string text, TimeSpan delay, params KeyboardKey[] keys)
	{
		Application.BringToFront();
		Focus();
		Input.Keyboard.SendInput(text, delay, keys);
		return this;
	}

	/// <summary>
	/// Focus the element then sends provided text as input. Can delay with before sending an optional set of key strokes.
	/// </summary>
	/// <param name="text"> The text to be sent. </param>
	/// <param name="delay"> An optional delay to wait before sending the provided keys. </param>
	/// <param name="keyStrokes"> An optional set of key strokes to be sent. </param>
	/// <returns> This <see cref="Element" /> instance. </returns>
	/// <exception cref="ArgumentException"> The text parameter is too long. </exception>
	public virtual Element SendInput(string text, TimeSpan delay, params KeyStroke[] keyStrokes)
	{
		Application.BringToFront();
		Focus();
		Input.Keyboard.SendInput(text, delay, keyStrokes);
		return this;
	}

	/// <summary>
	/// Focus the element then sends provided set of key as input.
	/// </summary>
	/// <param name="keys"> The set of keys to be sent. </param>
	/// <returns> This <see cref="Element" /> instance. </returns>
	public virtual Element SendInput(params KeyboardKey[] keys)
	{
		Application.BringToFront();
		Focus();
		Input.Keyboard.SendInput(keys);
		return this;
	}

	/// <summary>
	/// Sends provided set of keys as input with a modifier (ctrl, shift, etc).
	/// </summary>
	/// <param name="modifiers"> The modifier key(s). </param>
	/// <param name="keys"> The set of keys to be sent. </param>
	/// <returns> This <see cref="Element" /> instance. </returns>
	public virtual Element SendInput(KeyboardModifier modifiers, params KeyboardKey[] keys)
	{
		Application.BringToFront();
		Focus();
		Input.Keyboard.SendInput(modifiers, keys);
		return this;
	}

	/// <summary>
	/// Focus the element then send the key strokes as input.
	/// </summary>
	/// <param name="keyStrokes"> An set of keyboard key strokes to send. </param>
	public virtual Element SendInput(params KeyStroke[] keyStrokes)
	{
		Application.BringToFront();
		Focus();
		Input.Keyboard.SendInput(keyStrokes);
		return this;
	}

	/// <summary>
	/// Sleeps the executing thread to create a pause between simulated inputs.
	/// </summary>
	/// <param name="timeout"> The time to wait. </param>
	/// <returns> This <see cref="Element" /> instance. </returns>
	public virtual Element Sleep(TimeSpan timeout)
	{
		Thread.Sleep(timeout);
		return this;
	}

	/// <summary>
	/// Provides a string of details for the element.
	/// </summary>
	/// <returns> The string of element details. </returns>
	public abstract string ToDetailString();

	#endregion
}