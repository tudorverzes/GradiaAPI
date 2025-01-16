﻿using System.ComponentModel.DataAnnotations;

namespace boost_api.dto.appUserDto;

public class UserLoginDto
{
	[Required]
	public string? Username { get; set; }
	[Required]
	public string? Password { get; set; }
}