#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Speedy.Extensions;
using Speedy.Protocols.Nmea.Exceptions;
using Speedy.Protocols.Nmea.Messages;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea;

public class NmeaParser
{
	#region Fields

	private static readonly Dictionary<string, NmeaMessagePrefix> _messagePrefixByShortName;
	private static readonly Dictionary<NmeaMessagePrefix, string> _messagePrefixShortNames;
	private static readonly Dictionary<string, NmeaMessageType> _messageTypeByShortName;
	private static readonly Dictionary<NmeaMessageType, string> _messageTypeShortNames;
	private readonly Dictionary<NmeaMessageType, NmeaMessage> _parsers;
	private readonly object _parsersLock;
	private static readonly Regex _regex;

	#endregion

	#region Constructors

	public NmeaParser()
	{
		_parsers = new Dictionary<NmeaMessageType, NmeaMessage>();
		_parsersLock = new object();

		var values = Enum.GetValues(typeof(NmeaMessageType))
			.Cast<NmeaMessageType>()
			.Except(new[] { NmeaMessageType.Unknown });

		values.ForEach(AddMessageParser);
	}

	static NmeaParser()
	{
		_regex = new Regex(@"^[$](?<prefix>[\w]{2})(?<type>[\w]{3})[,]{1}", RegexOptions.Compiled | RegexOptions.Singleline);

		var prefixes = Enum.GetValues(typeof(NmeaMessagePrefix)).Cast<NmeaMessagePrefix>().Where(x => x != NmeaMessagePrefix.Unknown).ToArray();
		_messagePrefixByShortName = prefixes.ToDictionary(x => x.ToDisplayShortName(), x => x);
		_messagePrefixShortNames = prefixes.ToDictionary(x => x, x => x.ToDisplayShortName());

		var types = Enum.GetValues(typeof(NmeaMessageType)).Cast<NmeaMessageType>().Where(x => x != NmeaMessageType.Unknown).ToArray();
		_messageTypeByShortName = types.ToDictionary(x => x.ToDisplayShortName(), x => x);
		_messageTypeShortNames = types.ToDictionary(x => x, x => x.ToDisplayShortName());
	}

	#endregion

	#region Methods

	public void AddMessageParser(NmeaMessageType type)
	{
		lock (_parsersLock)
		{
			var message = CreateMessageParser(type);
			if (message == null)
			{
				return;
			}

			AddMessageParser(message);
		}
	}

	public void AddMessageParser(NmeaMessage message)
	{
		lock (_parsersLock)
		{
			if (_parsers.ContainsKey(message.Type))
			{
				var parser = _parsers[message.Type];
				parser.NmeaMessageParsed -= OnMessageParsed;
				_parsers.Remove(message.Type);
			}

			message.NmeaMessageParsed += OnMessageParsed;
			_parsers.AddOrUpdate(message.Type, message);
		}
	}

	public static (NmeaMessagePrefix prefix, NmeaMessageType type, string value) ExtractPrefixAndType(string sentence)
	{
		var result = _regex.Match(sentence);
		if (!result.Success)
		{
			return (NmeaMessagePrefix.Unknown, NmeaMessageType.Unknown, string.Empty);
		}

		var prefix = GetMessagePrefix(result.Groups["prefix"].Value);
		var type = GetMessageType(result.Groups["type"].Value);

		return (prefix, type, result.Value);
	}

	public static string GetSentenceStart(NmeaMessage message)
	{
		var prefix = _messagePrefixShortNames[message.Prefix];
		var type = _messageTypeShortNames[message.Type];
		return $"${prefix}{type}";
	}

	public NmeaMessage Parse(string sentence, DateTime? timestamp = null)
	{
		try
		{
			sentence = NmeaMessage.CleanupSentence(sentence);

			if (string.IsNullOrWhiteSpace(sentence))
			{
				return null;
			}

			if (sentence.Length < 6)
			{
				return null;
			}

			var (prefix, type, value) = ExtractPrefixAndType(sentence);

			lock (_parsersLock)
			{
				if (_parsers.ContainsKey(type))
				{
					var p = _parsers[type];
					if (p == null)
					{
						return null;
					}
					p.Parse(sentence);
					p.ReceivedOn = timestamp ?? TimeService.UtcNow;
					return p;
				}
			}

			return null;
		}
		catch (NmeaParseChecksumException)
		{
			return null;
		}
	}

	private NmeaMessage CreateMessageParser(NmeaMessageType type)
	{
		return type switch
		{
			NmeaMessageType.GBS => new GbsMessage(),
			NmeaMessageType.GGA => new GgaMessage(),
			NmeaMessageType.GLL => new GllMessage(),
			NmeaMessageType.GNS => new GnsMessage(),
			NmeaMessageType.GRS => new GrsMessage(),
			NmeaMessageType.GSA => new GsaMessage(),
			NmeaMessageType.GST => new GstMessage(),
			NmeaMessageType.GSV => new GsvMessage(),
			NmeaMessageType.RMC => new RmcMessage(),
			NmeaMessageType.TXT => new TxtMessage(),
			NmeaMessageType.VTG => new VtgMessage(),
			NmeaMessageType.ZDA => new ZdaMessage(),
			_ => null
		};
	}

	private static NmeaMessagePrefix GetMessagePrefix(string prefix)
	{
		prefix = prefix?.ToUpper() ?? string.Empty;
		return _messagePrefixByShortName.ContainsKey(prefix) ? _messagePrefixByShortName[prefix] : NmeaMessagePrefix.Unknown;
	}

	private static NmeaMessageType GetMessageType(string type)
	{
		type = type?.ToUpper() ?? string.Empty;
		return _messageTypeByShortName.ContainsKey(type) ? _messageTypeByShortName[type] : NmeaMessageType.Unknown;
	}

	private void OnMessageParsed(object sender, NmeaMessage e)
	{
		MessageParsed?.Invoke(this, e);
	}

	#endregion

	#region Events

	public event EventHandler<NmeaMessage> MessageParsed;

	#endregion
}