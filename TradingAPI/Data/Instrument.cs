using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TradingAPI.Data
{
    public class Instrument
    {
        [Key]
        public string Symbol { get; set; }//PK (symbol e.g.: BTC 4 lookups)
        public string SymbolChar { get; set; }
        public string SymbolName { get; set; }
        public string Type { get; set; }//?

    }
}
