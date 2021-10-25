#region References

using System.Collections.Concurrent;
using System.Linq;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	public class OscCommandProcessor
	{
		#region Fields

		private readonly ConcurrentDictionary<OscAddress, IOscMessageHandler> _handlers;

		#endregion

		#region Constructors

		public OscCommandProcessor()
		{
			_handlers = new ConcurrentDictionary<OscAddress, IOscMessageHandler>();
		}

		#endregion

		#region Methods

		public void Add(IOscMessageHandler handler)
		{
			_handlers.AddOrUpdate(handler.Address, x => handler, (x, h) => handler);
		}

		public bool Process(object sender, OscMessage message)
		{
			return _handlers.Values.Any(handler => handler.Process(sender, message));
		}

		public void Remove(IOscMessageHandler handler)
		{
			_handlers.TryRemove(handler.Address, out _);
		}

		#endregion
	}
}