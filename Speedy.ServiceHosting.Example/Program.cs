namespace Speedy.ServiceHosting.Example
{
	public static class Program
	{
		#region Methods

		private static void Main(string[] args)
		{
			var options = new ServiceOptions();
			options.Initialize(args);

			var service = new Service(options);
			service.Start();
		}

		#endregion
	}
}