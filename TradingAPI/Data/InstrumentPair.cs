using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TradingAPI.Data
{
    public class InstrumentPair
    {
        //public int Id { get; set; }//PK
        [Key]
        public string Symbol { get; set; }//(symbol: e.g: BTCUSDT 4 lookups)
        public Instrument BaseInstrument { get; set; }//FK
        public Instrument QuoteInstrument { get; set; }//FK
        public bool IceBergAllowed { get; set; }
        public bool IsSpotTradingAllowed { get; set; }
        public bool IsMarginTradingAllowed { get; set; }
        public bool OcoAllowed { get; set; }
        public bool QuoteOrderQuantityMarketAllowed { get; set; }
        public int BaseCommissionPrecision { get; set; }
        public int QuoteCommissionPrecision { get; set; }

    }
}
