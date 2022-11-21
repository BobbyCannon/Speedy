#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

public enum OscSlipBytes : byte
{
	End = 0xC0,
	Escape = 0xDB,
	EscapeEnd = 0xDC,
	EscapeEscape = 0xDD
}