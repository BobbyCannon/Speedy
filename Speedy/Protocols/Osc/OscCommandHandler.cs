#region References

using System;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	public class OscCommandHandler<T> : IOscMessageHandler where T : OscCommand, new()
	{
		#region Fields

		private readonly Func<object, T, bool> _handler;

		#endregion

		#region Constructors

		public OscCommandHandler(Func<object, T, bool> handler)
		{
			_handler = handler;

			Address = new OscAddress(GetAddress());
		}

		#endregion

		#region Properties

		public OscAddress Address { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Get the object this handler will process
		/// </summary>
		/// <param name="message"> </param>
		/// <returns> </returns>
		public virtual OscCommand GetModel(OscMessage message)
		{
			var type = new T();
			type.Load(message);
			return type;
		}

		public bool Matches(string address)
		{
			return Address.Matches(address);
		}

		public bool Matches(OscMessage message)
		{
			return Address.Matches(message.Address);
		}

		public bool Process(object sender, OscMessage message)
		{
			return Matches(message) && _handler(sender, (T) GetModel(message));
		}

		private static string GetAddress()
		{
			var type = new T();
			return type.Address;
		}

		#endregion
	}
}