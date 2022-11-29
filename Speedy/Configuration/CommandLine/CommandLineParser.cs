#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.Configuration.CommandLine;

/// <summary>
/// Represents a command line parser.
/// </summary>
public class CommandLineParser : Bindable, IEnumerable<CommandLineArgument>
{
	#region Fields

	private readonly IDictionary<string, CommandLineArgument> _arguments;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an instance of the command line parser.
	/// </summary>
	public CommandLineParser()
	{
		_arguments = new Dictionary<string, CommandLineArgument>();

		UnknownArguments = new Dictionary<string, string>();
	}

	#endregion

	#region Properties

	/// <summary>
	/// All arguments are valid.
	/// </summary>
	public bool IsValid => _arguments.All(x => x.Value.IsValid);

	/// <summary>
	/// Gets a command line argument by name.
	/// </summary>
	/// <param name="index"> The name of the argument. </param>
	/// <returns> The argument of the name requested. </returns>
	public CommandLineArgument this[string index] => _arguments[index];

	/// <summary>
	/// Arguments that are unknown by the loaded argument settings.
	/// </summary>
	public Dictionary<string, string> UnknownArguments { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Adds an argument to the command line parser.
	/// </summary>
	/// <param name="argument"> The argument to be added. </param>
	public void Add(CommandLineArgument argument)
	{
		if (argument == null)
		{
			throw new ArgumentNullException(nameof(argument), "The argument cannot be null.");
		}
		if (string.IsNullOrEmpty(argument.Name))
		{
			throw new ArgumentException("Argument name must be set.");
		}
		if (_arguments.ContainsKey(argument.Name))
		{
			throw new ArgumentException($"The argument [{argument.Name}] has been duplicated.");
		}
		_arguments.Add(argument.Name, argument);
	}

	/// <summary>
	/// Builds the help information that will be displayed with the -h command option is requested.
	/// </summary>
	/// <returns> The string to be displayed. </returns>
	public virtual string BuildHelpInformation(StringBuilder builder = null, Func<CommandLineArgument, bool> shouldIncludeCheck = null)
	{
		builder ??= new StringBuilder();

		foreach (var argument in this)
		{
			if ((shouldIncludeCheck != null) && !shouldIncludeCheck.Invoke(argument))
			{
				continue;
			}

			builder.AppendLine($"\t{argument.GetHelpDescription()}");
		}

		return builder.ToString();
	}

	/// <summary>
	/// Builds the issue information that will be displayed with the -h command option is requested.
	/// </summary>
	/// <returns> The string to be displayed. </returns>
	public virtual string BuildIssueInformation(StringBuilder builder = null)
	{
		builder ??= new StringBuilder();

		foreach (var argument in this.Where(x => !x.IsValid))
		{
			builder.AppendLine($"\t{argument.GetIssueDescription()}");
		}

		return builder.ToString();
	}

	/// <inheritdoc />
	public IEnumerator<CommandLineArgument> GetEnumerator()
	{
		return _arguments.Values.GetEnumerator();
	}

	/// <summary>
	/// Process a provided sets of arguments
	/// </summary>
	public void Parse(params string[] arguments)
	{
		Reset();

		for (var index = 0; index < arguments.Length; index++)
		{
			var indexValue = arguments[index];
			var nextValue = (index + 1) < arguments.Length ? arguments[index + 1] : null;
			var processed = false;

			foreach (var argument in _arguments.Values)
			{
				if (argument.WasFound)
				{
					continue;
				}

				if (argument.Process(indexValue))
				{
					processed = true;
					break;
				}

				if (argument.Process(indexValue, nextValue))
				{
					processed = true;
					index++;
					break;
				}
			}

			if (!processed)
			{
				if (indexValue.StartsWith("-") && (nextValue != null) && !nextValue.StartsWith("-"))
				{
					UnknownArguments.Add(indexValue, nextValue);
					index++;
				}
				else
				{
					UnknownArguments.Add(indexValue, null);
				}
			}
		}
	}

	/// <summary>
	/// Gets an argument by th. property name.
	/// </summary>
	/// <param name="name"> The name of the property. </param>
	/// <returns> The found command line argument otherwise null. </returns>
	public CommandLineArgument Property(string name)
	{
		return _arguments.Values.FirstOrDefault(x => x.PropertyName == name);
	}

	/// <summary>
	/// Gets an argument value the property name.
	/// </summary>
	/// <param name="name"> The name of the property. </param>
	/// <param name="defaultValue"> The default value if the argument was not found or the value was not provided. </param>
	/// <returns> The found command line value otherwise the default value provided. </returns>
	public T PropertyValue<T>(string name, T defaultValue = default)
	{
		var argument = _arguments.Values.FirstOrDefault(x => x.PropertyName == name);
		var value = argument?.Value;

		if ((value == null) && (argument?.HasDefaultValue == false))
		{
			return defaultValue;
		}

		var dValue = default(T);
		if (argument is CommandLineArgument<T> typedArgument && typedArgument.HasDefaultValue)
		{
			dValue = typedArgument.DefaultValue;
		}
		else if ((argument != null) && argument.HasDefaultValue)
		{
			dValue = argument.DefaultValue is T castValue ? castValue : default;
		}

		if (value == null)
		{
			return dValue;
		}

		return dValue switch
		{
			byte _ => byte.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			char _ => char.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			short _ => short.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			ushort _ => ushort.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			int _ => int.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			uint _ => uint.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			long _ => long.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			ulong _ => ulong.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			float _ => float.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			double _ => double.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			TimeSpan _ => TimeSpan.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			DateTime _ => DateTime.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			DateTimeOffset _ => DateTimeOffset.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			OscTimeTag _ => OscTimeTag.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			Version _ => Version.TryParse(value, out var pValue) ? (T) (object) pValue : dValue,
			string _ => value is T tValue ? tValue : dValue,
			_ => value is T tValue ? tValue : dValue
		};
	}

	/// <summary>
	/// Resets the command line parser.
	/// </summary>
	public virtual void Reset()
	{
		foreach (var argument in _arguments.Values)
		{
			argument.Reset();
		}

		UnknownArguments.Clear();
	}

	/// <summary>
	/// Gets the command line.
	/// </summary>
	/// <returns> The command line. </returns>
	public override string ToString()
	{
		var builder = new StringBuilder();

		foreach (var argument in this)
		{
			if (!argument.WasFound)
			{
				continue;
			}

			builder.Append(" ");
			builder.Append(argument);
		}

		foreach (var argument in UnknownArguments)
		{
			builder.Append(" ");
			builder.Append(argument.Key);

			if (argument.Value != null)
			{
				builder.Append(" ");
				builder.Append(argument.Value);
			}
		}

		builder.Remove(0, 1);
		return builder.ToString();
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion
}