using DataModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataAccess
{
    public class FaceRollContext : DbContext
    {
        public DbSet<Meeting> Meetings { get; set; }

        public DbSet<Attendee> Attendees { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=faceroll.db");
        }

        public static void InitializeDatabase()
        {
            using (var context = new FaceRollContext())
            {
                context.Database.EnsureCreated();
            }
        }
    }
}
