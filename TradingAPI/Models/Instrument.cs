using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;
using HotChocolate.Data;

namespace TradingAPI.Models
{
    public class Instrument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Symbol { get; set; }//PK (symbol e.g.: BTC 4 lookups)
        public string Name { get; set; }
        public string SymbolChar { get; set; }
        
        public string Type { get; set; }//?


        //[UseProjection]
        public virtual List<InstrumentPair> BaseInstrumentPairs { get; set; }

        //[UseProjection]
        public virtual List<InstrumentPair> QuoteInstrumentPairs { get; set; }
    }
}
