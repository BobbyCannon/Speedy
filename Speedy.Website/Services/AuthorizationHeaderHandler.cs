#region References

using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Speedy.Data;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Website.Services
{
	public class AuthorizationHeaderHandler : DelegatingHandler
	{
		#region Methods

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (!request.Headers.TryGetValues("Authorization", out var credentials))
			{
				return base.SendAsync(request, cancellationToken);
			}

			var credentialValues = credentials.First().Replace("Basic ", "").FromBase64().Split(':');
			if (credentialValues.Length != 2)
			{
				return base.SendAsync(request, cancellationToken);
			}

			var database = request.GetDependencyScope().GetService(typeof(IContosoDatabase)) as IContosoDatabase;
			var service = new AccountService(database, null);
			AccountEntity account;

			try
			{
				account = service.AuthenticateAccount(new Credentials { EmailAddress = credentialValues[0], Password = credentialValues[1] });
			}
			catch (AuthenticationException)
			{
				return base.SendAsync(request, cancellationToken);
			}

			var username = account.Id + ";" + account.Name;
			var usernameClaim = new Claim(ClaimTypes.Name, username);
			var identity = new ClaimsIdentity(new[] { usernameClaim }, "Basic");
			var roles = Roles.GetRolesForUser(username);
			var principal = new GenericPrincipal(identity, roles);

			HttpContext.Current.User = principal;

			return base.SendAsync(request, cancellationToken);
		}

		#endregion
	}
}