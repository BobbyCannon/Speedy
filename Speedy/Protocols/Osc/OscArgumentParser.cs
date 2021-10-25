#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	public class OscArgumentParser<T> : OscArgumentParser where T : IOscArgument, new()
	{
		#region Constructors

		public OscArgumentParser() : base(new T())
		{
		}

		#endregion

		#region Methods

		public override object Parse(byte[] buffer, ref int index)
		{
			var response = new T();
			response.ParseOscValue(buffer, ref index);
			return response;
		}

		public override object Parse(string value)
		{
			var response = new T();
			response.ParseOscValue(value);
			return response;
		}

		#endregion
	}

	public abstract class OscArgumentParser
	{
		#region Fields

		private readonly char _binaryType;
		private readonly string _stringType;

		#endregion

		#region Constructors

		protected OscArgumentParser(IOscArgument argument)
		{
			_binaryType = argument.GetOscBinaryType();
			_stringType = argument.GetOscStringType();
		}

		#endregion

		#region Methods

		public bool CanParse(char value)
		{
			return _binaryType == value;
		}

		public bool CanParse(string value)
		{
			return _stringType == value;
		}

		public abstract object Parse(byte[] buffer, ref int index);

		public abstract object Parse(string value);

		#endregion
	}
}