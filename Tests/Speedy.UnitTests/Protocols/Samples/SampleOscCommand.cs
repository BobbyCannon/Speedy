#region References

using System;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Samples
{
	public class SampleOscCommand : OscCommand
	{
		#region Constants

		public const string Command = "/sample";

		#endregion

		#region Constructors

		public SampleOscCommand() : base(Command, 3)
		{
		}

		#endregion

		#region Properties

		public DateTime BirthDate { get; set; }

		public bool Enabled { get; set; }

		public string Name { get; set; }

		public OscTimeTag Timestamp { get; set; }

		public SampleCustomValue Value { get; set; }

		#endregion

		#region Methods

		protected override void LoadMessage()
		{
			StartArgumentProcessing();

			Version = GetArgument<int>();

			if (Version >= 1)
			{
				// You can read argument by type
				Name = GetArgument<string>();
			}

			if (Version >= 2)
			{
				// You can also read arguments using provided named methods
				BirthDate = GetArgumentAsDateTime();
				Enabled = GetArgumentAsBoolean();
			}
			else
			{
				// Load old version defaults here!
				BirthDate = DateTime.MinValue;
				Enabled = false;
			}

			if (Version >= 3)
			{
				Value = GetArgument<SampleCustomValue>();
				Timestamp = GetArgument<OscTimeTag>();
			}
			else
			{
				// Load old version defaults here!
				Value = new SampleCustomValue(0, 0, 0);
				Timestamp = OscTimeTag.MinValue;
			}
		}

		protected override void UpdateMessage()
		{
			switch (Version)
			{
				case 0:
				case 1:
					SetArguments(Version, Name);
					break;

				case 2:
					SetArguments(Version, Name, BirthDate, Enabled);
					break;

				// Always default to the latest
				case 3:
				default:
					SetArguments(Version, Name, BirthDate, Enabled, Value, Timestamp);
					break;
			}
		}

		#endregion
	}
}