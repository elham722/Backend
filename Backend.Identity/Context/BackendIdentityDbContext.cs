using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Identity.Configurations;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Identity.Context
{
    public class BackendIdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<string>, string, UserClaim, UserRole, UserLogin, IdentityRoleClaim<string>, UserToken>
    {
        public BackendIdentityDbContext(DbContextOptions<BackendIdentityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply configurations
            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new UserClaimConfiguration());
            builder.ApplyConfiguration(new UserLoginConfiguration());
            builder.ApplyConfiguration(new UserRoleConfiguration());
            builder.ApplyConfiguration(new UserTokenConfiguration());

            // Add composite indexes for better performance
            AddCompositeIndexes(builder);
        }

        private void AddCompositeIndexes(ModelBuilder builder)
        {
            // Composite indexes for common queries
            builder.Entity<UserClaim>()
                .HasIndex(uc => new { uc.UserId, uc.IsActive })
                .HasDatabaseName("IX_AspNetUserClaims_UserId_IsActive_Composite");

            builder.Entity<UserLogin>()
                .HasIndex(ul => new { ul.UserId, ul.IsActive })
                .HasDatabaseName("IX_AspNetUserLogins_UserId_IsActive_Composite");

            builder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.IsActive, ur.ExpiresAt })
                .HasDatabaseName("IX_AspNetUserRoles_UserId_IsActive_ExpiresAt_Composite");

            builder.Entity<UserToken>()
                .HasIndex(ut => new { ut.UserId, ut.IsActive, ut.IsRevoked })
                .HasDatabaseName("IX_AspNetUserTokens_UserId_IsActive_IsRevoked_Composite");

            // Indexes for audit queries
            builder.Entity<UserClaim>()
                .HasIndex(uc => new { uc.CreatedBy, uc.CreatedAt })
                .HasDatabaseName("IX_AspNetUserClaims_CreatedBy_CreatedAt_Audit");

            builder.Entity<UserLogin>()
                .HasIndex(ul => new { ul.CreatedBy, ul.CreatedAt })
                .HasDatabaseName("IX_AspNetUserLogins_CreatedBy_CreatedAt_Audit");

            builder.Entity<UserRole>()
                .HasIndex(ur => new { ur.AssignedBy, ur.AssignedAt })
                .HasDatabaseName("IX_AspNetUserRoles_AssignedBy_AssignedAt_Audit");

            builder.Entity<UserToken>()
                .HasIndex(ut => new { ut.CreatedBy, ut.CreatedAt })
                .HasDatabaseName("IX_AspNetUserTokens_CreatedBy_CreatedAt_Audit");
        }
    }
}
