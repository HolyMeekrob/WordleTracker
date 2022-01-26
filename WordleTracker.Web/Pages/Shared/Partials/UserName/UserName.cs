using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WordleTracker.Web.Pages.Shared.Partials;
using WordleTracker.Web.Utilities;

namespace WordleTracker.Web.Pages;

public partial class PartialsModel
{
	private async Task<IActionResult> UpdateNameAsync(UserNameModel model, CancellationToken cancellationToken)
	{
		try
		{
			model.UserId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

			ModelState.Clear();
			TryValidateModel(model);

			if (!ModelState.IsValid)
			{
				model.UserName = User.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;
				return Partial("UserName", model);
			}

			var user = await _userSvc.UpdateName(model.UserId, model.UserName, cancellationToken);

			var identity = User.Identity as ClaimsIdentity;
			identity!.RemoveClaim(identity.FindFirst(ClaimTypes.Name));
			identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));

			await Authentication.SignOut(HttpContext);
			await Authentication.SignIn(HttpContext, user);

			return Partial("UserName", new UserNameModel(user));
		}
		catch (Exception)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, "Server error saving user name");
		}
	}
}
