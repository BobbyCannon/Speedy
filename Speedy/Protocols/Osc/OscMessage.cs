#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Speedy.Extensions;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	/// <summary>
	/// Represents an OSC message.
	/// </summary>
	public class OscMessage : OscPacket, IEnumerable<object>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of an OSC message for the provided address and arguments.
		/// </summary>
		/// <param name="address"> The address. </param>
		/// <param name="args"> The arguments. </param>
		/// <remarks>
		/// Do NOT call this constructor with an object[] unless you want a message with a single
		/// object of type object[]. Because an object[] is an object the parameter is seen as a
		/// single entry array.
		/// </remarks>
		public OscMessage(string address, params object[] args)
			: this(TimeService.UtcNow, address, null, args)
		{
		}

		/// <summary>
		/// Instantiates an instance of an OSC message for the provided address and arguments.
		/// </summary>
		/// <param name="time"> The time. </param>
		/// <param name="address"> The address. </param>
		/// <param name="args"> The arguments. </param>
		/// <remarks>
		/// Do NOT call this constructor with an object[] unless you want a message with a single
		/// object of type object[]. Because an object[] is an object the parameter is seen as a
		/// single entry array.
		/// </remarks>
		public OscMessage(DateTime time, string address, params object[] args)
			: this(time, address, null, args)
		{
		}

		/// <summary>
		/// Instantiates an instance of an OSC message for the provided address and arguments.
		/// </summary>
		/// <param name="time"> The time. </param>
		/// <param name="address"> The address. </param>
		/// <param name="dispatcher"> The dispatcher for updates. </param>
		/// <param name="args"> The arguments. </param>
		/// <remarks>
		/// Do NOT call this constructor with an object[] unless you want a message with a single
		/// object of type object[]. Because an object[] is an object the parameter is seen as a
		/// single entry array.
		/// </remarks>
		public OscMessage(DateTime time, string address, IDispatcher dispatcher, params object[] args) : base(dispatcher)
		{
			Address = address;
			Arguments = new List<object>();
			Arguments.AddRange(args);
			Time = time;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The address of the message.
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// The arguments of this message.
		/// </summary>
		public List<object> Arguments { get; set; }

		/// <summary>
		/// The number of arguments in the message.
		/// </summary>
		public int Count => Arguments.Count;

		/// <summary>
		/// The argument list is empty.
		/// </summary>
		public bool IsEmpty => !Arguments.Any();

		/// <summary>
		/// Access Arguments by index
		/// </summary>
		/// <param name="index"> the index of the argument </param>
		/// <returns> argument at the supplied index </returns>
		public object this[int index] => Arguments[index];

		#endregion

		#region Methods

		/// <summary>
		/// They'll be times when you want to instantiate an message with an actually object array. Used this factory method. If you pass
		/// an object[] to the constructor it will actually be an object[] with one entry of that object[].
		/// </summary>
		/// <param name="address"> The address. </param>
		/// <param name="args"> The arguments. </param>
		/// <returns> The message for the address and arguments. </returns>
		public static OscMessage FromObjectArray(string address, IEnumerable<object> args)
		{
			return FromObjectArray(TimeService.UtcNow, address, args);
		}

		/// <summary>
		/// They'll be times when you want to instantiate an message with an actually object array. Used this factory method. If you pass
		/// an object[] to the constructor it will actually be an object[] with one entry of that object[].
		/// </summary>
		/// <param name="time"> The time. </param>
		/// <param name="address"> The address. </param>
		/// <param name="args"> The arguments. </param>
		/// <returns> The message for the address and arguments. </returns>
		public static OscMessage FromObjectArray(DateTime time, string address, IEnumerable<object> args)
		{
			var response = new OscMessage(time, address);
			response.Arguments.AddRange(args);
			return response;
		}

		/// <summary>
		/// Gets the argument as the specified type. Does a direct cast so if the type is wrong then it will exception.
		/// </summary>
		/// <typeparam name="T"> The type the argument is. </typeparam>
		/// <param name="index"> The index of the argument to cast. </param>
		/// <param name="defaultValue"> The default value if the argument index does not exists. </param>
		/// <returns> The typed argument. </returns>
		public T GetArgument<T>(int index, T defaultValue = default)
		{
			if (index >= Arguments.Count)
			{
				return defaultValue;
			}

			return (T) Arguments[index];
		}

		/// <inheritdoc />
		public IEnumerator<object> GetEnumerator()
		{
			return Arguments.GetEnumerator();
		}

		/// <summary>
		/// Sets the arguments for the message.
		/// </summary>
		/// <param name="arguments"> The arguments to be set. </param>
		public void SetArguments(params object[] arguments)
		{
			Arguments.Clear();
			Arguments.AddRange(arguments);
		}

		/// <summary>
		/// Convert the message to a byte array.
		/// </summary>
		/// <returns> The bytes that represents the message. </returns>
		public override byte[] ToByteArray()
		{
			var parts = new List<byte[]>();
			var currentList = Arguments;
			var argumentsIndex = 0;
			var typeString = ",";
			var i = 0;

			while (i < currentList.Count)
			{
				var arg = currentList[i];
				switch (arg)
				{
					case short sArg:
					{
						typeString += "i";
						parts.Add(OscBitConverter.GetBytes(sArg));
						break;
					}
					case ushort usArg:
					{
						typeString += "i";
						parts.Add(OscBitConverter.GetBytes(usArg));
						break;
					}
					case int iArg:
					{
						typeString += "i";
						parts.Add(OscBitConverter.GetBytes(iArg));
						break;
					}
					case uint uiArg:
					{
						typeString += "u";
						parts.Add(OscBitConverter.GetBytes(uiArg));
						break;
					}
					case long i64:
					{
						typeString += "h";
						parts.Add(OscBitConverter.GetBytes(i64));
						break;
					}
					case ulong ui64:
					{
						typeString += "H";
						parts.Add(OscBitConverter.GetBytes(ui64));
						break;
					}
					case float sArg:
					{
						if (float.IsPositiveInfinity(sArg) || float.IsNegativeInfinity(sArg))
						{
							typeString += "I";
						}
						else
						{
							typeString += "f";
							parts.Add(OscBitConverter.GetBytes(sArg));
						}
						break;
					}
					case double dValue:
					{
						if (double.IsPositiveInfinity(dValue) || double.IsNegativeInfinity(dValue))
						{
							typeString += "I";
						}
						else
						{
							typeString += "d";
							parts.Add(OscBitConverter.GetBytes(dValue));
						}
						break;
					}
					case decimal dValue:
					{
						typeString += "M";
						parts.Add(OscBitConverter.GetBytes(dValue));
						break;
					}
					case byte bValue:
					{
						typeString += "c";
						parts.Add(OscBitConverter.GetBytes((char) bValue));
						break;
					}
					case sbyte bValue:
					{
						typeString += "c";
						parts.Add(OscBitConverter.GetBytes((char) bValue));
						break;
					}
					case char character:
					{
						typeString += "c";
						parts.Add(OscBitConverter.GetBytes(character));
						break;
					}
					case bool boolean:
					{
						typeString += boolean ? "T" : "F";
						break;
					}
					case null:
					{
						typeString += "N";
						break;
					}
					case string s:
					{
						typeString += "s";
						parts.Add(OscBitConverter.GetBytes(s));
						break;
					}
					case IOscArgument oscType:
					{
						typeString += oscType.GetOscBinaryType();
						parts.Add(oscType.GetOscValueBytes());
						break;
					}
					case DateTime time:
					{
						typeString += "t";
						var oscTime = new OscTimeTag(time);
						parts.Add(OscBitConverter.GetBytes(oscTime.Value));
						break;
					}
					case TimeSpan timeSpan:
					{
						typeString += "p";
						parts.Add(OscBitConverter.GetBytes(timeSpan.Ticks));
						break;
					}
					case OscSymbol s2Value:
					{
						typeString += "S";
						parts.Add(OscBitConverter.GetBytes(s2Value.Value));
						break;
					}
					case OscCrc crc:
					{
						typeString += "C";
						parts.Add(OscBitConverter.GetBytes(crc));
						break;
					}
					case byte[] b:
					{
						typeString += "b";
						parts.Add(OscBitConverter.GetBytes(b));
						break;
					}
					// Guid types that are just converted to strings and back.
					case Guid value:
					{
						typeString += "s";
						parts.Add(OscBitConverter.GetBytes(value.ToString()));
						break;
					}
					case Enum eArg:
					{
						typeString += "i";
						parts.Add(OscBitConverter.GetBytes(Convert.ToInt32(eArg)));
						break;
					}
					// This part handles arrays. It points currentList to the array and reSets i
					// The array is processed like normal and when it is finished we replace  
					// currentList back with Arguments and continue from where we left off
					case IEnumerable<object> objects:
					{
						if (Arguments != currentList)
						{
							throw new Exception("Nested Arrays are not supported");
						}
						typeString += "[";
						currentList = objects.ToList();
						argumentsIndex = i;
						i = 0;
						continue;
					}
					default:
					{
						throw new Exception("Unable to transmit values of type " + arg.GetType());
					}
				}

				i++;

				if ((currentList != Arguments) && (i == currentList.Count))
				{
					// End of array, go back to main Argument list
					typeString += "]";
					currentList = Arguments;
					i = argumentsIndex + 1;
				}
			}

			var addressLen = string.IsNullOrEmpty(Address) ? 0 : Address.AlignedStringLength();
			var typeLen = typeString.AlignedStringLength();
			var total = addressLen + typeLen + parts.Sum(x => x.Length);
			var output = new byte[total];
			i = 0;

			Encoding.ASCII.GetBytes(Address).CopyTo(output, i);
			i += addressLen;

			Encoding.ASCII.GetBytes(typeString).CopyTo(output, i);
			i += typeLen;

			foreach (var part in parts)
			{
				part.CopyTo(output, i);
				i += part.Length;
			}

			return output;
		}

		/// <summary>
		/// Converts the message to a HEX string.
		/// </summary>
		/// <returns> A hex string format of the message. </returns>
		public string ToHexString()
		{
			return ToString(CultureInfo.InvariantCulture, true);
		}

		/// <summary>
		/// Converts the message to a string.
		/// </summary>
		/// <returns> A string format of the message. </returns>
		public override string ToString()
		{
			return ToString(CultureInfo.InvariantCulture, false);
		}

		/// <summary>
		/// Converts the message to a string.
		/// </summary>
		/// <returns> A string format of the message. </returns>
		public string ToString(IFormatProvider provider, bool numberAsHex)
		{
			var sb = new StringBuilder();

			sb.Append(Address);

			if (IsEmpty)
			{
				return sb.ToString();
			}

			sb.Append(",");

			ArgumentsToString(sb, numberAsHex, provider, Arguments);

			return sb.ToString();
		}

		internal static void ArgumentsToString(StringBuilder sb, bool hex, IFormatProvider provider, IEnumerable<object> args)
		{
			var first = true;

			foreach (var obj in args)
			{
				if (first == false)
				{
					sb.Append(",");
				}
				else
				{
					first = false;
				}

				switch (obj)
				{
					case int i:
					{
						sb.Append(hex ? $"0x{i.ToString("X8", provider)}" : i.ToString(provider));
						break;
					}
					case uint u:
					{
						sb.Append(hex ? $"0x{u.ToString("X8", provider)}u" : $"{u.ToString(provider)}u");
						break;
					}
					case long l:
					{
						sb.Append(hex ? $"0x{l.ToString("X16", provider)}L" : $"{l.ToString(provider)}L");
						break;
					}
					case ulong ul:
					{
						sb.Append(hex ? $"0x{ul.ToString("X16", provider)}U" : $"{ul.ToString(provider)}U");
						break;
					}
					case float f:
					{
						var value = f;

						if (float.IsInfinity(value) || float.IsNaN(value))
						{
							sb.Append(f.ToString(provider));
						}
						else
						{
							sb.Append(f.ToString(provider) + "f");
						}
						break;
					}
					case double d:
					{
						sb.Append(d.ToString(provider) + "d");
						break;
					}
					case decimal d:
					{
						sb.Append(d.ToString(provider) + "m");
						break;
					}
					case byte b:
					{
						sb.Append($"'{(char) b}'");
						break;
					}
					case char c:
					{
						sb.Append($"'{c}'");
						break;
					}
					case bool b:
					{
						sb.Append(b.ToString());
						break;
					}
					case null:
					{
						sb.Append("null");
						break;
					}
					case string value:
					{
						sb.Append($"\"{value.Escape()}\"");
						break;
					}
					case IOscArgument oscType:
					{
						sb.Append($"{{ {oscType.GetOscStringType()}: {oscType.GetOscValueString()} }}");
						break;
					}
					case DateTime dateTime:
					{
						var dateTimeString = dateTime.ToUtcString();
						sb.Append($"{{ Time: {dateTimeString} }}");
						break;
					}
					case TimeSpan timeSpan:
					{
						sb.Append($"{{ TimeSpan: {timeSpan} }}");
						break;
					}
					case OscSymbol symbol:
					{
						sb.Append(symbol.Value.Escape());
						break;
					}
					case byte[] bytes:
					{
						sb.Append($"{{ Blob: {bytes.ToStringBlob()} }}");
						break;
					}
					case Guid value:
					{
						sb.Append($"\"{value}\"");
						break;
					}
					case Enum eValue:
					{
						sb.Append(Convert.ToInt32(eValue));
						break;
					}
					case IEnumerable<object> objects:
					{
						sb.Append('[');
						ArgumentsToString(sb, hex, provider, objects);
						sb.Append(']');
						break;
					}
					default:
					{
						sb.Append(obj.ToString().Escape());
						break;
					}
				}
			}
		}

		/// <summary>
		/// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
		/// </summary>
		/// <param name="time"> The created time of the message. </param>
		/// <param name="data"> The data for the message. </param>
		/// <param name="length"> The length for the message. </param>
		/// <param name="parsers"> An optional set of OSC argument parsers. </param>
		/// <returns> Message containing various arguments and an address </returns>
		internal static OscPacket ParseMessage(DateTime time, byte[] data, int length, params OscArgumentParser[] parsers)
		{
			var index = 0;
			var arguments = new List<object>();
			var mainArray = arguments; // used as a reference when we are parsing arrays to Get the main array back

			// Get address
			var address = GetAddress(data, index);
			index += data.FirstIndexAfter(address.Length, x => x == ',');

			if ((index % 4) != 0)
			{
				return new OscError(TimeService.UtcNow, OscError.Message.InvalidMessageAddressMisAligned);
			}

			// Get type tags
			var types = GetTypes(data, index);
			index += types.Length;

			while ((index % 4) != 0)
			{
				index++;
			}

			var commaParsed = false;

			//
			// Use type 'char' values
			//
			//	\0, i (int32), u (uint32), f (float), s (string), b (blob), h (int64), H (uint64),
			//	t (OscTime), p (TimeSpan), d (double), S (symbol), c (char), r (rgba), m (midi),
			//	T (true), F (false), N (null), I (positive infinity), C (crc), M (decimal)
			//	[ (start array), ] (end array)
			//

			foreach (var type in types)
			{
				// skip leading comma
				if ((type == ',') && !commaParsed)
				{
					commaParsed = true;
					continue;
				}

				switch (type)
				{
					case '\0':
						break;

					case 'i':
						var iValue = OscBitConverter.ToInt32(data, index);
						arguments.Add(iValue);
						index += 4;
						break;

					case 'u':
						var uValue = OscBitConverter.ToUInt32(data, index);
						arguments.Add(uValue);
						index += 4;
						break;

					case 'f':
						var fValue = OscBitConverter.ToFloat(data, index);
						arguments.Add(fValue);
						index += 4;
						break;

					case 's':
						var sValue = OscBitConverter.ToString(data, ref index);
						arguments.Add(sValue);
						break;

					case 'b':
						var bValue = OscBitConverter.ToBlob(data, index);
						arguments.Add(bValue);
						index += 4 + bValue.Length;
						break;

					case 'h':
						var hValue = OscBitConverter.ToInt64(data, index);
						arguments.Add(hValue);
						index += 8;
						break;

					case 'H':
						var ulValue = OscBitConverter.ToUInt64(data, index);
						arguments.Add(ulValue);
						index += 8;
						break;

					case 't':
						var tValue = OscBitConverter.ToUInt64(data, index);
						arguments.Add(new OscTimeTag(tValue));
						index += 8;
						break;

					case 'p':
						var tsValue = OscBitConverter.ToInt64(data, index);
						arguments.Add(new TimeSpan(tsValue));
						index += 8;
						break;

					case 'd':
					{
						var dValue = OscBitConverter.ToDouble(data, index);
						arguments.Add(dValue);
						index += 8;
						break;
					}
					case 'M':
					{
						var dValue = OscBitConverter.ToDecimal(data, index);
						arguments.Add(dValue);
						index += 16;
						break;
					}
					case 'S':
						var s2Value = OscBitConverter.ToString(data, ref index);
						arguments.Add(new OscSymbol(s2Value));
						break;

					case 'c':
						var cValue = OscBitConverter.ToChar(data, index);
						arguments.Add(cValue);
						index += 4;
						break;

					case 'r':
						var rValue = OscBitConverter.ToOscType<OscRgba>(data, ref index);
						arguments.Add(rValue);
						break;

					case 'm':
						var mValue = OscBitConverter.ToOscType<OscMidi>(data, ref index);
						arguments.Add(mValue);
						break;

					case 'T':
						arguments.Add(true);
						break;

					case 'F':
						arguments.Add(false);
						break;

					case 'N':
						arguments.Add(null);
						break;

					case 'I':
						arguments.Add(double.PositiveInfinity);
						break;

					case 'C':
						var crcValue = OscBitConverter.ToCrc(data, index);
						arguments.Add(crcValue);
						index += 2;
						break;

					case '[':
						if (arguments != mainArray)
						{
							return new OscError(TimeService.UtcNow, OscError.Message.UnsupportedNestedArrays);
						}
						arguments = new List<object>(); // make arguments point to a new object array
						break;

					case ']':
						mainArray.Add(arguments); // add the array to the main array
						arguments = mainArray; // make arguments point back to the main array
						break;

					default:
						var parsed = false;

						foreach (var parser in parsers)
						{
							if (!parser.CanParse(type))
							{
								continue;
							}

							var value = parser.Parse(data, ref index);
							arguments.Add(value);
							parsed = true;
							break;
						}

						if (!parsed)
						{
							return new OscError(TimeService.UtcNow, OscError.Message.UnknownTagType, type);
						}

						break;
				}

				while ((index % 4) != 0)
				{
					index++;
				}

				if (index > length)
				{
					break;
				}
			}

			return new OscMessage(time, address, arguments.ToArray());
		}

		/// <summary>
		/// Parse a message from a string using the supplied provider.
		/// </summary>
		/// <param name="time"> The time to represent the message. </param>
		/// <param name="value"> A string containing the OSC message data. </param>
		/// <param name="provider"> The format provider to use during parsing. </param>
		/// <param name="parsers"> An optional set of OSC argument parsers. </param>
		/// <returns> The parsed OSC message. </returns>
		internal static OscPacket ParseMessage(DateTime time, string value, IFormatProvider provider, params OscArgumentParser[] parsers)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return new OscError(TimeService.UtcNow, OscError.Message.InvalidParseOscPacketInput);
			}

			var index = value.IndexOf(',');

			if (index <= 0)
			{
				// could be an argument less message				
				index = value.Length;
			}

			var address = value.Substring(0, index).Trim();

			if (string.IsNullOrWhiteSpace(address))
			{
				return new OscError(TimeService.UtcNow, OscError.Message.InvalidMessageAddressWasEmpty);
			}

			if (OscAddress.IsValidAddress(address) == false)
			{
				return new OscError(TimeService.UtcNow, OscError.Message.InvalidMessageAddress);
			}

			var arguments = new List<object>();

			try
			{
				Extensions.ParseArguments(value, arguments, index + 1, provider, parsers);
			}
			catch (Exception ex)
			{
				return new OscError(TimeService.UtcNow, OscError.Message.FailedParsingArguments, ex.Message);
			}

			return new OscMessage(time, address, arguments.ToArray());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}