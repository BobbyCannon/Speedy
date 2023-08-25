#region References

using System;
using Speedy.Exceptions;

#endregion

namespace Speedy.Collections;

/// <summary>
/// A little endian specific buffer.
/// </summary>
/// <remarks>
/// <see cref="EndianBuffer" />
/// </remarks>
public class LittleEndianBuffer : EndianBuffer
{
	#region Constructors

	/// <summary>
	/// Instantiates a little endian buffer.
	/// </summary>
	/// <param name="size"> The size of the buffer. </param>
	public LittleEndianBuffer(int size) : base(size, true)
	{
	}

	/// <summary>
	/// Load a buffer that contains data in a little endian order.
	/// </summary>
	/// <param name="buffer"> The buffer to load. </param>
	/// <returns> The little endian buffer. </returns>
	public LittleEndianBuffer(byte[] buffer) : this(buffer.Length)
	{
		LoadBuffer(buffer);
	}

	#endregion
}

/// <summary>
/// A big endian specific buffer.
/// </summary>
/// <remarks>
/// <see cref="EndianBuffer" />
/// </remarks>
public class BigEndianBuffer : EndianBuffer
{
	#region Constructors

	/// <summary>
	/// Instantiates a big endian buffer.
	/// </summary>
	/// <param name="size"> The size of the buffer. </param>
	public BigEndianBuffer(int size) : base(size, false)
	{
	}

	/// <summary>
	/// Load a buffer that contains data in a big endian order.
	/// </summary>
	/// <param name="buffer"> The buffer to load. </param>
	/// <returns> The big endian buffer. </returns>
	public BigEndianBuffer(byte[] buffer) : this(buffer.Length)
	{
		LoadBuffer(buffer);
	}

	#endregion
}

/// <summary>
/// A endian specific buffer.
/// </summary>
/// <remarks>
/// Little-endian byte ordering places the least significant byte first. This method is used in Intel microprocessors, for example.
/// Big-endian byte ordering places the most significant byte first. This method is used in IBM mainframes and Motorola microprocessors, for example.
/// Network byte order is defined to always be big-endian.
/// </remarks>
public abstract class EndianBuffer
{
	#region Fields

	private readonly byte[] _buffer;
	private int _lastWroteIndex;
	private int _readIndex;
	private int _writeIndex;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates a buffer order specifically for a specific endianness (byte-order). All writes and
	/// reads will conform to the selected endianness.
	/// </summary>
	/// <param name="size"> The size of the buffer. </param>
	/// <param name="isLittleEndian"> True to be little endian otherwise big endian. </param>
	protected EndianBuffer(int size, bool isLittleEndian)
	{
		_buffer = new byte[size];
		_lastWroteIndex = 0;

		IsLittleEndian = isLittleEndian;
	}

	#endregion

	#region Properties

	/// <summary>
	/// If true this buffer is in LittleEndian order else BigEndian order.
	/// </summary>
	public bool IsLittleEndian { get; }

	/// <summary>
	/// The length of the endian buffer.
	/// </summary>
	public int Length => _buffer.Length;

	/// <summary>
	/// The index of the read.
	/// </summary>
	public int ReadIndex
	{
		get => _readIndex;
		set
		{
			if (value < 0)
			{
				_readIndex = 0;
			}
			else if (value > _buffer.Length)
			{
				_readIndex = _buffer.Length;
			}
			else
			{
				_readIndex = value;
			}
		}
	}

	/// <summary>
	/// The index of the write.
	/// </summary>
	public int WriteIndex
	{
		get => _writeIndex;
		set
		{
			if (value < 0)
			{
				_writeIndex = 0;
			}
			else if (value > _buffer.Length)
			{
				_writeIndex = _buffer.Length;
			}
			else
			{
				_writeIndex = value;
			}
		}
	}

	#endregion

	#region Methods

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="index"> The index to start reading from. </param>
	/// <param name="length"> The amount of data to read. </param>
	/// <returns> The array of byte data. </returns>
	public byte[] ReadArray(int index, int length)
	{
		var response = new byte[length];
		if (!TryRead(index, response, length, IsLittleEndian))
		{
			throw new SpeedyException("Failed to read the array.");
		}

		return response;
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="index"> The index to start reading from. </param>
	/// <returns> The array of byte data. </returns>
	public byte ReadByte(int index)
	{
		if ((index < 0) || (index >= _buffer.Length))
		{
			throw new ArgumentOutOfRangeException(nameof(index), "The index is out of range of the buffer.");
		}

		return _buffer[index];
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <returns> True if the read was successful otherwise false. </returns>
	public short ReadInt16()
	{
		return ReadInt16(ReadIndex);
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="index"> The index to read from. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public short ReadInt16(int index)
	{
		if (!TryReadInt16(index, out var value))
		{
			throw new SpeedyException("Failed to read the int16 value.");
		}

		return value;
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <returns> True if the read was successful otherwise false. </returns>
	public int ReadInt32()
	{
		return ReadInt32(ReadIndex);
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="index"> The index to read from. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public int ReadInt32(int index)
	{
		if (!TryReadInt32(index, out var value))
		{
			throw new SpeedyException("Failed to read the int32 value.");
		}

		return value;
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <returns> True if the read was successful otherwise false. </returns>
	public ushort ReadUInt16()
	{
		return ReadUInt16(ReadIndex);
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="index"> The index to read from. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public ushort ReadUInt16(int index)
	{
		if (!TryReadUInt16(index, out var value))
		{
			throw new SpeedyException("Failed to read the uint16 value.");
		}

		return value;
	}

	/// <summary>
	/// Converts the buffer to an array.
	/// </summary>
	/// <returns> The buffer as a byte array. </returns>
	public byte[] ToArray()
	{
		var response = new byte[_buffer.Length];
		Array.Copy(_buffer, 0, response, 0, response.Length);
		return response;
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="value"> The value that was read if successful. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public bool TryReadInt16(out short value)
	{
		return TryReadInt16(ReadIndex, out value);
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="index"> The index to read from. </param>
	/// <param name="value"> The value that was read if successful. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public bool TryReadInt16(int index, out short value)
	{
		var data = new byte[2];

		if (TryRead(index, data, data.Length, BitConverter.IsLittleEndian))
		{
			value = BitConverter.ToInt16(data, 0);
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="value"> The value that was read if successful. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public bool TryReadInt32(out int value)
	{
		return TryReadInt32(ReadIndex, out value);
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="index"> The index to read from. </param>
	/// <param name="value"> The value that was read if successful. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public bool TryReadInt32(int index, out int value)
	{
		var data = new byte[4];

		if (TryRead(index, data, data.Length, BitConverter.IsLittleEndian))
		{
			value = BitConverter.ToInt32(data, 0);
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="value"> The value that was read if successful. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public bool TryReadInt64(out long value)
	{
		var data = new byte[8];

		if (TryRead(data, data.Length, BitConverter.IsLittleEndian))
		{
			value = BitConverter.ToInt64(data, 0);
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="value"> The value that was read if successful. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public bool TryReadUInt16(out ushort value)
	{
		return TryReadUInt16(ReadIndex, out value);
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value that was read if successful. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public bool TryReadUInt16(int index, out ushort value)
	{
		var data = new byte[2];

		if (TryRead(index, data, data.Length, BitConverter.IsLittleEndian))
		{
			value = BitConverter.ToUInt16(data, 0);
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="value"> The value that was read if successful. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public bool TryReadUInt32(out uint value)
	{
		var data = new byte[4];

		if (TryRead(data, data.Length, BitConverter.IsLittleEndian))
		{
			value = BitConverter.ToUInt32(data, 0);
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="value"> The value that was read if successful. </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	public bool TryReadUInt64(out ulong value)
	{
		var data = new byte[8];

		if (TryRead(data, data.Length, BitConverter.IsLittleEndian))
		{
			value = BitConverter.ToUInt64(data, 0);
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Write an array af values into the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteArray(byte[] value)
	{
		return TryWriteArray(WriteIndex, value);
	}

	/// <summary>
	/// Write an array af values into the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteArray(int index, byte[] value)
	{
		return TryWrite(index, value, IsLittleEndian);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteInt16(short value)
	{
		return TryWriteInt16(WriteIndex, value);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteInt16(int index, short value)
	{
		var data = BitConverter.GetBytes(value);
		return TryWrite(index, data, BitConverter.IsLittleEndian);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteInt32(int value)
	{
		return TryWriteInt32(WriteIndex, value);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteInt32(int index, int value)
	{
		var data = BitConverter.GetBytes(value);
		return TryWrite(index, data, BitConverter.IsLittleEndian);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteInt64(long value)
	{
		return TryWriteInt64(WriteIndex, value);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteInt64(int index, long value)
	{
		var data = BitConverter.GetBytes(value);
		return TryWrite(index, data, BitConverter.IsLittleEndian);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteUInt16(ushort value)
	{
		return TryWriteUInt16(WriteIndex, value);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteUInt16(int index, ushort value)
	{
		var data = BitConverter.GetBytes(value);
		return TryWrite(index, data, BitConverter.IsLittleEndian);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteUInt32(uint value)
	{
		var data = BitConverter.GetBytes(value);
		return TryWrite(data, BitConverter.IsLittleEndian);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public bool TryWriteUInt64(ulong value)
	{
		var data = BitConverter.GetBytes(value);
		return TryWrite(data, BitConverter.IsLittleEndian);
	}

	/// <summary>
	/// Write an array af values into the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteArray(byte[] value)
	{
		WriteArray(WriteIndex, value);
	}

	/// <summary>
	/// Write an array af values into the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteArray(int index, byte[] value)
	{
		if (!TryWriteArray(index, value))
		{
			throw new SpeedyException("Failed to write the array value.");
		}
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="index"> The index to write the value to. </param>
	/// <param name="value"> The value to be written. </param>
	public void WriteByte(int index, byte value)
	{
		ValidateIndex(index);

		_buffer[index] = value;
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteInt16(short value)
	{
		if (!TryWriteInt16(value))
		{
			throw new SpeedyException("Failed to write the int16 value.");
		}
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteInt16(int index, short value)
	{
		if (!TryWriteInt16(index, value))
		{
			throw new SpeedyException("Failed to write the int16 value.");
		}
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteInt32(int value)
	{
		if (!TryWriteInt32(value))
		{
			throw new SpeedyException("Failed to write the int32 value.");
		}
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteInt32(int index, int value)
	{
		if (!TryWriteInt32(index, value))
		{
			throw new SpeedyException("Failed to write the int32 value.");
		}
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteInt64(long value)
	{
		if (!TryWriteInt64(value))
		{
			throw new SpeedyException("Failed to write the int64 value.");
		}
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteInt64(int index, long value)
	{
		if (!TryWriteInt64(index, value))
		{
			throw new SpeedyException("Failed to write the int64 value.");
		}
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteUInt16(ushort value)
	{
		WriteUInt16(WriteIndex, value);
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteUInt16(int index, ushort value)
	{
		if (!TryWriteUInt16(index, value))
		{
			throw new SpeedyException("Failed to write the uint16 value.");
		}
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteUInt32(uint value)
	{
		if (!TryWriteUInt32(value))
		{
			throw new SpeedyException("Failed to write the uint32 value.");
		}
	}

	/// <summary>
	/// Write the value to the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	public void WriteUInt64(ulong value)
	{
		if (!TryWriteUInt64(value))
		{
			throw new SpeedyException("Failed to write the uint64 value.");
		}
	}

	/// <summary>
	/// Load full array into the buffer.
	/// </summary>
	/// <param name="buffer"> The buffer to load. </param>
	/// <exception cref="ArgumentException"> The buffer length is invalid. </exception>
	protected void LoadBuffer(byte[] buffer)
	{
		if (buffer.Length != _buffer.Length)
		{
			throw new ArgumentException("The buffer length is invalid.", nameof(Buffer));
		}

		Array.Copy(buffer, 0, _buffer, 0, buffer.Length);

		_lastWroteIndex = _buffer.Length;
	}

	/// <summary>
	/// Write an array af values into the buffer.
	/// </summary>
	/// <param name="value"> The value to write. </param>
	/// <param name="valueIsLittleEndian"> The value being written is little endian. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	protected bool TryWrite(byte[] value, bool valueIsLittleEndian)
	{
		return TryWrite(WriteIndex, value, valueIsLittleEndian);
	}

	/// <summary>
	/// Write an array af values into the buffer.
	/// </summary>
	/// <param name="index"> The index to write to. </param>
	/// <param name="value"> The value to write. </param>
	/// <param name="valueIsLittleEndian"> The value being written is little endian. </param>
	/// <returns> True if the write was successful otherwise false. </returns>
	protected bool TryWrite(int index, byte[] value, bool valueIsLittleEndian)
	{
		var remainingBufferLength = _buffer.Length - index;
		var length = Math.Min(value.Length, remainingBufferLength);

		if ((length == 0) || (length < value.Length))
		{
			// There nothing to do or not enough space left.
			return false;
		}

		// If both these do not match then we must reverse order
		if (valueIsLittleEndian != IsLittleEndian)
		{
			// Reverse write th buffer
			for (var i = length - 1; i >= 0; i--)
			{
				var bufferOffset = value.Length - i - 1;
				_buffer[bufferOffset + index] = value[i];
			}
		}
		else
		{
			// Buffers are in same order so just write the value.
			Array.Copy(value, 0, _buffer, index, length);
		}

		var newWriteIndex = index + length;

		WriteIndex = newWriteIndex;

		if (newWriteIndex > _lastWroteIndex)
		{
			_lastWroteIndex = newWriteIndex;
		}

		return true;
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="value"> The value that was read if successful. </param>
	/// <param name="length"> The amount of data to read. </param>
	/// <param name="wantsLittleEndian"> True to read the value as little endian otherwise big endian </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	private bool TryRead(byte[] value, int length, bool wantsLittleEndian)
	{
		return TryRead(ReadIndex, value, length, wantsLittleEndian);
	}

	/// <summary>
	/// Read the value from the buffer.
	/// </summary>
	/// <param name="index"> The index to start reading from. </param>
	/// <param name="value"> The value that was read if successful. </param>
	/// <param name="length"> The amount of data to read. </param>
	/// <param name="wantsLittleEndian"> True to read the value as little endian otherwise big endian </param>
	/// <returns> True if the read was successful otherwise false. </returns>
	private bool TryRead(int index, byte[] value, int length, bool wantsLittleEndian)
	{
		if (value.Length < length)
		{
			return false;
		}

		var remainingBuffer = _lastWroteIndex - index;
		length = Math.Min(length, remainingBuffer);

		if (length < value.Length)
		{
			// There not enough buffer left to read..
			return false;
		}

		// If both value match then just copy
		if (wantsLittleEndian == IsLittleEndian)
		{
			// Order is same as requested so just copy
			Array.Copy(_buffer, index, value, 0, length);
		}
		else
		{
			for (var i = length - 1; i >= 0; i--)
			{
				var bufferOffset = value.Length - i - 1;
				value[i] = _buffer[bufferOffset + index];
			}
		}

		ReadIndex = index + length;

		return true;
	}

	private void ValidateIndex(int index)
	{
		if ((index < 0) || (index >= _buffer.Length))
		{
			throw new ArgumentOutOfRangeException(nameof(index), "The index is out of range of the buffer.");
		}
	}

	#endregion
}