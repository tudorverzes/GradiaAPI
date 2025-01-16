using boost_api.model;

namespace boost_api.service;

public interface ITokenService
{
	string CreateToken(AppUser user, bool isAdmin = false);
}