﻿using System.Data.Entity;
using Domain.Entities;

namespace Domain.EF
{
    public class UserContext : DbContext
    {
        public UserContext()
        {
            Database.Log = s => System.Diagnostics.Trace.WriteLine(s);
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<UserContext>());
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<UserProduct> UserProducts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ProvidersProductInfo> ProvidersProductInfos { get; set; }
        public DbSet<Article> Articles { get; set; }

    }
}
