using boost_api.model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace boost_api.data;

public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
	public DbSet<Chat> Chats { get; set; }
	public DbSet<Message> Messages { get; set; }
	public DbSet<Analysis> Analyses { get; set; }
	
	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.Entity<Message>()
			.HasOne(m => m.Analysis);
		
		List<IdentityRole> roles =
		[
			new IdentityRole
			{
				Name = "Admin",
				NormalizedName = "ADMIN"
			},

			new IdentityRole
			{
				Name = "User",
				NormalizedName = "USER"
			}

		];
		builder.Entity<IdentityRole>().HasData(roles);
		
	}
}