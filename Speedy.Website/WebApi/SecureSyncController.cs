#region References

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Speedy.Samples;

#endregion

namespace Speedy.Website.WebApi
{
	[Authorize]
	[ApiController]
	[Route("api/SecureSync")]
	public class SecureSyncController : BaseEntitySyncController
	{
		#region Constructors

		public SecureSyncController(IDatabaseProvider<IContosoDatabase> provider) : base(provider)
		{
		}

		#endregion
	}
}