using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightenceServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LightenceServer.Data
{
    public class AppDbContext: IdentityDbContext<LightUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) 
        { 

        }

        public DbSet<ProductKeyModel> ProductKeys { get; set; }

        public DbSet<ServerLogModel> ServerLogs { get; set; }

        public DbSet<LightFaceModel> BiometricLogins { get; set; }

    }
}
