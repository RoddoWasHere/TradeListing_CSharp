using Binance.Net.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TradingAPI.Models
{
    public class PriceHistory
    {
        public int Id { get; set; }

        public string InstrumentPairId { get; set; }
        public InstrumentPair InstrumentPair { get; set; }

        public KlineInterval Interval { get; set; }

        public long UtcOpenTime { get; set; }
        public long UtcCloseTime { get; set; }

        [Column(TypeName = "decimal(10,10)")]
        public decimal High { get; set; }
        [Column(TypeName = "decimal(10,10)")]
        public decimal Low { get; set; }
        [Column(TypeName = "decimal(10,10)")]
        public decimal Open { get; set; }
        [Column(TypeName = "decimal(10,10)")]
        public decimal Close { get; set; }

        public int TradeCount { get; set; }


    }
}
