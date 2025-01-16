using Microsoft.AspNetCore.Identity;

namespace boost_api.model;

public class AppUser : IdentityUser
{
	public List<Chat> Chats { get; set; } = [];
}