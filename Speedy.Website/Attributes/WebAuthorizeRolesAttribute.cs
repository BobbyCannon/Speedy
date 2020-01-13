#region References

using System.Web.Http;
using Speedy.Website.Samples.Enumerations;

#endregion

namespace Speedy.Website.Attributes
{
	public class WebAuthorizeRolesAttribute : AuthorizeAttribute
	{
		#region Constructors

		public WebAuthorizeRolesAttribute(params AccountRole[] roles)
		{
			Roles = string.Join(",", roles);
		}

		#endregion
	}
}