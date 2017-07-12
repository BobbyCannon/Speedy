#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.EntityFramework.Migrations
{
	internal sealed class Configuration : DbMigrationsConfiguration<ContosoDatabase>
	{
		#region Constructors

		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}

		#endregion

		#region Methods

		protected override void Seed(ContosoDatabase context)
		{
		}

		#endregion
	}
}