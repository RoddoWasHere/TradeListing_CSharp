using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingAPI.Models;

namespace TradingAPI.Data {
    public class TradeListingDbContext : DbContext {
        public TradeListingDbContext(DbContextOptions<TradeListingDbContext> options) : base(options) 
        {

        }

        public DbSet<Instrument> Instrument { get; set; }
        public DbSet<InstrumentPair> InstrumentPair { get; set; }
        public DbSet<PriceHistory> PriceHistory { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<InstrumentPair>().HasKey(i => i.Symbol);
            modelBuilder.Entity<InstrumentPair>().
                HasOne(p => p.BaseInstrument).WithMany(i => i.BaseInstrumentPairs).HasForeignKey(p => p.BaseInstrumentId);

            modelBuilder.Entity<InstrumentPair>().
                HasOne(p => p.QuoteInstrument).WithMany(i => i.QuoteInstrumentPairs).HasForeignKey(p => p.QuoteInstrumentId);


            modelBuilder.Entity<PriceHistory>().HasKey(h => h.Id);
            modelBuilder.Entity<PriceHistory>().
                HasOne(h => h.InstrumentPair).WithMany(p => p.PriceHistory).HasForeignKey(h => h.InstrumentPairId);

        }
    }
}