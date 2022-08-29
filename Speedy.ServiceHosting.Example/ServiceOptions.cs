#region References

using System;

#endregion

namespace Speedy.ServiceHosting.Example
{
	/// <summary>
	/// The options for the service.
	/// </summary>
	public class ServiceOptions : WindowsServiceOptions
	{
		#region Constructors

		public ServiceOptions() : base(Guid.Parse("BA2ABAEC-8D3C-4F98-9599-BFF546269367"), "Service", "Service Hosting")
		{
		}

		#endregion

		#region Properties

		public string Message => PropertyValue<string>(nameof(Message));

		public int Start => PropertyValue<int>(nameof(Start));

		#endregion

		#region Methods

		public override void SetupArguments()
		{
			Add(new WindowsServiceArgument<int>
			{
				Help = "The offset to start with.",
				Name = nameof(Start).ToLower(),
				PropertyName = nameof(Start),
				DefaultValue = 1
			});

			Add(new WindowsServiceArgument<string>
			{
				Help = "A require message to print.",
				Name = "m",
				PropertyName = nameof(Message),
				IncludeInServiceArguments = true,
				IsRequired = true
			});

			base.SetupArguments();
		}

		#endregion
	}
}