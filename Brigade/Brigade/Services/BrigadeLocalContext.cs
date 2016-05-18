using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Brigade.Models;

namespace Brigade.Services
{
	public class BrigadeLocalContext : DbContext
	{
		public DbSet<Asset> Assets { get; set; }
		public DbSet<Models.Brigade> Brigades { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<UserTask> Tasks { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Filename=Brigade.db");
		}
	}
}
