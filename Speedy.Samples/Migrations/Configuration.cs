#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.Migrations
{
	internal sealed class Configuration : DbMigrationsConfiguration<EntityFrameworkSampleDatabase>
	{
		#region Constructors

		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}

		#endregion

		#region Methods

		protected override void Seed(EntityFrameworkSampleDatabase context)
		{
		}

		#endregion
	}
}