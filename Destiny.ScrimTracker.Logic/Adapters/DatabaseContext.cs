using System;
using Destiny.ScrimTracker.Logic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace Destiny.ScrimTracker.Logic.Adapters
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<GuardianEfficiency> GuardianEfficiencies { get; set; }
        public DbSet<GuardianElo> GuardianElos { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchTeam> MatchTeams { get; set; }
        public DbSet<GuardianMatchResult> GuardianMatchResults { get; set; }
        
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Guardian Property Bindings
            modelBuilder.Entity<Guardian>().HasIndex(g => g.GamerTag).IsUnique();
            
            // GuardianELO Property Bindings 
            modelBuilder.Entity<GuardianElo>()
                .HasOne<Guardian>()
                .WithMany()
                .HasForeignKey(elo => elo.GuardianId);
            
            modelBuilder.Entity<GuardianElo>().Property(elo => elo.TimeStamp).HasDefaultValueSql("now()");
            
            // GuardianEfficiency Property Bindings
            modelBuilder.Entity<GuardianEfficiency>()
                .HasOne<Guardian>()
                .WithMany()
                .HasForeignKey(eff => eff.GuardianId);
            
            modelBuilder.Entity<GuardianEfficiency>().Property(eff => eff.TimeStamp).HasDefaultValueSql("now()");
            
            // Match Model Bindings
            modelBuilder.Entity<Match>().Property(m => m.TimeStamp).HasDefaultValueSql("now()");

            // MatchTeam Model Bindings
            modelBuilder.Entity<MatchTeam>()
                .HasOne<Match>()
                .WithMany()
                .HasForeignKey(mt => mt.MatchId);
            
            modelBuilder.Entity<MatchTeam>().Ignore(mt => mt.GuardianMatchResults);

            // GuardianMatchResult Model Bindings
            modelBuilder.Entity<GuardianMatchResult>()
                .HasOne<MatchTeam>()
                .WithMany()
                .HasForeignKey(gmr => gmr.MatchTeamId);

            modelBuilder.Entity<GuardianMatchResult>()
                .HasOne<Match>()
                .WithMany()
                .HasForeignKey(gmr => gmr.MatchId);
        }
    }
}