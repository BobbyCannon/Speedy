#region References

using System;
using System.IO.Ports;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Samples
{
	public class TestOscCommand : OscCommand
	{
		#region Constants

		public const string Command = "/test";

		#endregion

		#region Constructors

		public TestOscCommand() : base(Command, 3)
		{
		}

		#endregion

		#region Properties

		public int Age { get; set; }

		public DateTime BirthDate { get; set; }

		public TimeSpan Elapsed { get; set; }

		public bool Enable { get; set; }

		public SerialError Error { get; set; }

		public float Height { get; set; }

		public ulong Id { get; set; }

		public string Name { get; set; }

		public byte Rating { get; set; }

		public short ShortId { get; set; }

		public Guid SyncId { get; set; }

		public ushort UShortId { get; set; }

		public byte[] Values { get; set; }

		public uint Visits { get; set; }

		public long VoteId { get; set; }

		public double Weight { get; set; }

		#endregion

		#region Methods

		protected override void LoadMessage()
		{
			StartArgumentProcessing();

			Version = GetArgument<int>();
			Id = GetArgument<ulong>();
			Name = GetArgument<string>();
			Age = GetArgument<int>();
			BirthDate = GetArgument<DateTime>();
			Height = GetArgument<float>();
			Weight = GetArgument<double>();
			Rating = GetArgument<byte>();
			Values = GetArgument<byte[]>();
			Enable = GetArgument<bool>();
			SyncId = GetArgument<Guid>();
			Visits = GetArgument<uint>();
			VoteId = GetArgument<long>();

			if (Version >= 2)
			{
				Elapsed = GetArgument<TimeSpan>();
				Error = GetArgument<SerialError>();
			}
			else
			{
				// Default to whatever you want
				Elapsed = TimeSpan.FromSeconds(3);
				Error = SerialError.Overrun;
			}

			if (Version >= 3)
			{
				ShortId = GetArgument<short>();
				UShortId = GetArgument<ushort>();
			}
		}

		protected override void UpdateMessage()
		{
			switch (Version)
			{
				case 1:
					// You can set the message
					OscMessage = new OscMessage(Time, Command, Version, Id, Name, Age, BirthDate, Height, Weight, Rating, Values, Enable, SyncId, Visits, VoteId);
					break;

				case 2:
					// You can also use "SetArguments" to only set values
					SetArguments(Version, Id, Name, Age, BirthDate, Height, Weight, Rating, Values, Enable, SyncId, Visits, VoteId, Elapsed, Error);
					break;

				case 3:
				default:
					// You can also use "SetArguments" to only set values
					SetArguments(Version, Id, Name, Age, BirthDate, Height, Weight, Rating, Values, Enable, SyncId, Visits, VoteId, Elapsed, Error, ShortId, UShortId);
					break;
			}
		}

		#endregion
	}
}