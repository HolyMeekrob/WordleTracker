using System.ComponentModel.DataAnnotations;
using WordleTracker.Data.Models;

namespace WordleTracker.Web.Pages.Shared.Partials;

public class UserNameModel
{
	public UserNameModel()
	{
	}

	public UserNameModel(string userId, string userName)
	{
		UserId = userId;
		UserName = userName;
	}

	public UserNameModel(User user) : this(user.Id, user.Name)
	{
	}

	public string UserId { get; set; } = null!;

	[Required(ErrorMessage = "Name cannot be blank")]
	[MaxLength(20, ErrorMessage = "Name must not be longer than 20 characters")]
	public string UserName { get; set; } = null!;
}
