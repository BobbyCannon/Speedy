#region References

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Speedy.Samples;

#endregion

namespace Speedy.Website.WebApi
{
	[AllowAnonymous]
	[ApiController]
	[Route("api/Sync")]
	public class SyncController : BaseEntitySyncController
	{
		#region Constructors

		public SyncController(IDatabaseProvider<IContosoDatabase> provider) : base(provider)
		{
		}

		#endregion
	}
}