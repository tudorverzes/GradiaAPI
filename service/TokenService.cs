﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using boost_api.model;
using Microsoft.IdentityModel.Tokens;

namespace boost_api.service;

public class TokenService : ITokenService
{
	private readonly IConfiguration _config;
	private readonly SymmetricSecurityKey _key;

	public TokenService(IConfiguration config)
	{
		_config = config;
		_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"] ?? string.Empty));
	}

	public string CreateToken(AppUser user, bool isAdmin = false)
	{
		var claims = new List<Claim>
		{
			new Claim("userId", user.Id),
			new Claim("role", isAdmin ? "admin" : "user")
		};

		var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = DateTime.Now.AddDays(7),
			SigningCredentials = creds,
			Issuer = _config["JWT:Issuer"],
			Audience = _config["JWT:Audience"]
		};

		var tokenHandler = new JwtSecurityTokenHandler();

		var token = tokenHandler.CreateToken(tokenDescriptor);

		return tokenHandler.WriteToken(token);
	}
}