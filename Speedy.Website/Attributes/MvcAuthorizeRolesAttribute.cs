#region References

#endregion

using Speedy.Website.Samples.Enumerations;

namespace Speedy.Website.Attributes
{
	public class MvcAuthorizeRolesAttribute : MvcAuthorizeAttribute
	{
		#region Constructors

		public MvcAuthorizeRolesAttribute(params AccountRole[] roles)
		{
			Roles = string.Join(",", roles);
		}

		public MvcAuthorizeRolesAttribute(params string[] roles)
		{
			Roles = string.Join(",", roles);
		}

		#endregion
	}
}