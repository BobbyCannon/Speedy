#region References

using System;
using Speedy.Extensions;

#endregion

namespace Speedy.Configuration.CommandLine
{
	/// <summary>
	/// Class managing the argument information
	/// </summary>
	public class CommandLineArgument<T> : CommandLineArgument
	{
		#region Properties

		/// <summary>
		/// The default value for arguments if not provided.
		/// </summary>
		public new T DefaultValue
		{
			get => base.DefaultValue is T ? (T) base.DefaultValue : default;
			set => base.DefaultValue = value;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override string ToString()
		{
			return WasFound
				? HasValue
					? typeof(T) == typeof(string)
						? $"{Prefix}{Name} \"{Value.Escape()}\""
						: $"{Prefix}{Name} {Value}"
					: $"{Prefix}{Name}"
				: string.Empty;
		}

		#endregion
	}

	/// <summary>
	/// Class managing the argument information
	/// </summary>
	public class CommandLineArgument : Bindable
	{
		#region Fields

		private object _defaultValue;
		private string _value;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an instance of a command line argument.
		/// </summary>
		public CommandLineArgument()
		{
			Prefix = "-";
		}

		#endregion

		#region Properties

		/// <summary>
		/// The default value for arguments if not provided.
		/// </summary>
		public object DefaultValue
		{
			get => _defaultValue;
			set
			{
				_defaultValue = value;
				HasDefaultValue = true;
			}
		}

		/// <summary>
		/// The argument has a default value.
		/// </summary>
		public bool HasDefaultValue { get; protected set; }

		/// <summary>
		/// True if the argument has a value.
		/// </summary>
		public bool HasValue { get; private set; }

		/// <summary>
		/// The help description.
		/// </summary>
		public string Help { get; set; }

		/// <summary>
		/// The argument represents just an existing flag with no following value.
		/// </summary>
		public bool IsFlag { get; set; }

		/// <summary>
		/// True if the argument is required.
		/// </summary>
		public bool IsRequired { get; set; }

		/// <summary>
		/// The argument was found and is flag or has value.
		/// </summary>
		public bool IsValid => !IsRequired || (IsRequired && WasFound && (HasValue || IsFlag));

		/// <summary>
		/// The name of the argument.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The prefix of the command line argument. Defaults to "-".
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// An optional property name for locating arguments.
		/// </summary>
		public string PropertyName { get; set; }

		/// <summary>
		/// The value parse from the command line.
		/// </summary>
		public string Value
		{
			get => _value;
			set
			{
				_value = value;
				HasValue = true;
			}
		}

		/// <summary>
		/// True if the argument was found.
		/// </summary>
		public bool WasFound { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the help description for argument.
		/// </summary>
		/// <returns> The description for the argument. </returns>
		public string GetHelpDescription()
		{
			return $"[{Prefix}{Name}] {Help}";
		}

		/// <summary>
		/// Gets the issue description for argument.
		/// </summary>
		/// <returns> The description of the issue for the argument. </returns>
		public string GetIssueDescription()
		{
			if (IsFlag && IsRequired && !WasFound)
			{
				return $"The required flag [{Name}] was not found.";
			}

			if (!IsFlag && IsRequired)
			{
				if (!WasFound)
				{
					return $"The required argument [{Name}] was not found.";
				}

				if (!HasValue)
				{
					return $"The required argument [{Name}] value was not provided";
				}
			}

			return "Unknown";
		}

		/// <summary>
		/// Try and parse the argument with out a value.
		/// </summary>
		/// <param name="argument"> The argument name with prefix. </param>
		/// <returns> True if processed otherwise false. </returns>
		public bool Process(string argument)
		{
			if (WasFound)
			{
				return false;
			}

			if (!IsFlag || ($"{Prefix}{Name}" != argument))
			{
				return false;
			}

			WasFound = true;
			return WasFound;
		}

		/// <summary>
		/// Try and parse the argument with a value.
		/// </summary>
		/// <param name="argument"> The argument name with prefix. </param>
		/// <param name="value"> The argument value. </param>
		/// <returns> True if processed otherwise false. </returns>
		public bool Process(string argument, string value)
		{
			// Cannot process if
			//  - already process (WasFound)
			//  - is a "flag" argument, meaning should not have a value
			//  - value is not provided AND there is no default value
			//  - value starts with the Prefix (ex -,--,/)
			if (WasFound || IsFlag || ((value == null) && !HasDefaultValue) || (value?.StartsWith(Prefix) == true))
			{
				return false;
			}

			if (!$"{Prefix}{Name}".Equals(argument, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			Value = value;
			WasFound = true;
			return true;
		}

		/// <summary>
		/// Resets the argument state.
		/// </summary>
		public virtual void Reset()
		{
			WasFound = false;
			Value = null;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return WasFound
				? HasValue
					? $"{Prefix}{Name} {Value}"
					: $"{Prefix}{Name}"
				: string.Empty;
		}

		#endregion
	}
}