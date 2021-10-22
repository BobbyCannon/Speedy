﻿#region References

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

		public int Start => PropertyValue(nameof(Start), 0);

		#endregion

		#region Methods

		public override void SetupArguments()
		{
			Add(new WindowsServiceArgument
			{
				Help = "The offset to start with.",
				IncludeInServiceArguments = false,
				Name = nameof(Start).ToLower(),
				PropertyName = nameof(Start)
			});

			base.SetupArguments();
		}

		#endregion
	}
}