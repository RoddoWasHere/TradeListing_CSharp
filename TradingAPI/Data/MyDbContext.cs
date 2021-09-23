using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradingAPI.Data {
    public class MyDbContext : DbContext {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) 
        {

        }

        public DbSet<Person> Person { get; set; }

        public DbSet<Instrument> Instrument { get; set; }
        public DbSet<InstrumentPair> InstrumentPair { get; set; }


        public DbSet<Student> Students { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Course> Courses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<InstrumentPair>()
                .HasOne<Instrument>(p => p.BaseInstrument);
                //.WithMany(i => i.BaseInstrumentPairs);
            //.HasForeignKey(p => p.BaseInstrument);

            modelBuilder.Entity<InstrumentPair>()
                .HasOne<Instrument>(p => p.QuoteInstrument);
                //.WithMany(i => i.QuoteInstrumentPairs);
            //.HasForeignKey(p => p.QuoteInstrument);

            //modelBuilder.Entity<Instrument>()
            //    .HasMany<InstrumentPair>(p => p.BaseInstrumentPairs)
            //    .WithOne(i => i.BaseInstrument);
            ////.HasForeignKey(p => p.BaseInstrument);

            //modelBuilder.Entity<Instrument>()
            //    .HasMany<InstrumentPair>(p => p.QuoteInstrumentPairs)
            //    .WithOne(i => i.QuoteInstrument);
            //    //.HasForeignKey(p => p.QuoteInstrument);


            //modelBuilder.Entity<InstrumentPair>()
            //    .HasIndex(i => new { i.QuoteI, t.CourseId })
            //    .IsUnique();


            modelBuilder.Entity<Student>()
                .HasMany(t => t.Enrollments)
                .WithOne(t => t.Student)
                .HasForeignKey(t => t.StudentId);

            modelBuilder.Entity<Enrollment>()
                .HasIndex(t => new { t.StudentId, t.CourseId })
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasMany(t => t.Enrollments)
                .WithOne(t => t.Course)
                .HasForeignKey(t => t.CourseId);
        }
    }
}