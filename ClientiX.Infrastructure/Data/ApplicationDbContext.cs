using ClientiX.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<MasterUser> MasterUsers { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<ClientBot> ClientBots { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MasterUser>(entity =>
            {
                entity.HasKey(e => e.TelegramUserId);
                entity.Property(e => e.TelegramUserId).ValueGeneratedNever();

                entity.HasOne(e => e.Subscription)
                    .WithOne(e => e.MasterUser)
                    .HasForeignKey<MasterUser>(e => e.SubscriptionId);

                entity.HasOne(e => e.ClientBot)
                    .WithOne(e => e.MasterUser)
                    .HasForeignKey<MasterUser>(e => e.ClientBotId);
            });

            modelBuilder.Entity<ClientBot>(entity =>
            {
                entity.HasKey(e => e.TelegramBotId);
                entity.Property(e => e.TelegramBotId).ValueGeneratedNever();
                entity.Property(e => e.ConfigJson).HasMaxLength(8000);
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.HasIndex(e => e.BookingDateTime);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
