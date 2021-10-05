using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TradingAPI.Models
{
    public class InstrumentPair
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Symbol { get; set; }//(symbol: e.g: BTCUSDT 4 lookups)

        public string BaseInstrumentId { get; set; }//FK
        public string QuoteInstrumentId { get; set; }//FK

        public virtual Instrument BaseInstrument { get; set; }//FK
        public virtual Instrument QuoteInstrument { get; set; }//FK

        public bool IceBergAllowed { get; set; }
        public bool IsSpotTradingAllowed { get; set; }
        public bool IsMarginTradingAllowed { get; set; }
        public bool OcoAllowed { get; set; }
        public bool QuoteOrderQuantityMarketAllowed { get; set; }
        public int BaseCommissionPrecision { get; set; }
        public int QuoteCommissionPrecision { get; set; }

        public List<PriceHistory> PriceHistory { get; set; }

    }
}
