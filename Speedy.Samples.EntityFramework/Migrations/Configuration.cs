#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.EntityFramework.Migrations
{
	internal sealed class Configuration : DbMigrationsConfiguration<EntityFrameworkContosoDatabase>
	{
		#region Constructors

		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}

		#endregion

		#region Methods

		protected override void Seed(EntityFrameworkContosoDatabase context)
		{
		}

		#endregion
	}
}