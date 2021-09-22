using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradingAPI.Data;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using Binance.Net.Objects.Spot.MarketData;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Other;

namespace TradingAPI.Controllers
{
    //[Route("api/[controller]")]
    [Route("")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly MyDbContext _context;

        private readonly BinanceClient _client;
        public PeopleController(MyDbContext context)
        {
            _context = context;

            _client = new BinanceClient(new BinanceClientOptions()
            {
                // Specify options for the client
            });

            //client.Spot.System.GetExchangeInfoAsync;

        }

        [HttpGet("exchangeInfo")]
        public async Task<ActionResult<BinanceExchangeInfo>> GetExchangeInfo()
        {
            WebCallResult<BinanceExchangeInfo> exchangeInfo = await _client.Spot.System.GetExchangeInfoAsync();

            return exchangeInfo.Data;
            //return await _context.Person.ToListAsync();
        }

        [HttpGet("exchangeInfoSave")]
        public async Task<ActionResult<BinanceExchangeInfo>> GetExchangeInfoSave()
        {
            WebCallResult<BinanceExchangeInfo> exchangeInfo = await _client.Spot.System.GetExchangeInfoAsync();


            //fetch exchange info



            return exchangeInfo.Data;
            //return await _context.Person.ToListAsync();
        }


        [HttpGet("prices")]
        public async Task<IEnumerable<BinancePrice>> GetPrices()
        {
            //WebCallResult<BinanceExchangeInfo> exchangeInfo = await _client.Spot.System.GetExchangeInfoAsync();
            //WebCallResult<IEnumerable<IBinanceTick>> tickers = await _client.Spot.Market.GetTickersAsync();
            WebCallResult<IEnumerable<BinancePrice>> prices = await _client.Spot.Market.GetPricesAsync(); //
            //BinancePrice p = new BinancePrice{p};
            //if (tickers.Success)
            //    return tickers;
            //var tickers = await _client.Spot.Market.GetTickersAsync();
            //return new ActionResult<IEnumerable<IBinanceTick>>(tickers.Data);
            //return await _context.Person.ToListAsync();
            return prices.Data;
        }

        [HttpGet("symbol/{symbol}")]
        public async Task<ActionResult<BinanceExchangeInfo>> GetSymbolInfo(string symbol)
        {
            WebCallResult<BinanceExchangeInfo> exchangeInfo = await _client.Spot.System.GetExchangeInfoAsync(symbol);
           
            return exchangeInfo.Data;
            //return await _context.Person.ToListAsync();
        }

        [HttpGet("coins")]
        public async Task<IEnumerable<BinanceProduct>> GetCoins()
        {
            WebCallResult<IEnumerable<BinanceProduct>> products = await _client.General.GetProductsAsync();

            return products.Data;
            //return await _context.Person.ToListAsync();
        }


        [HttpGet("coinsSave")]
        public async Task<IEnumerable<InstrumentPair>> GetCoinsAndSave()//test
        {

            bool shouldFetchFromAPI = _context.Instrument.Count() == 0 || _context.InstrumentPair.Count() == 0;

            List<Instrument> curInstruments = _context.Instrument.ToList();
            List<InstrumentPair> curPairs = _context.InstrumentPair.ToList();

            Dictionary<string, InstrumentPair> curPairsLookup = curPairs.ToDictionary(i => i.Symbol);
            Dictionary<string, Instrument> curInstrLookup = curInstruments.ToDictionary(i => i.Symbol);

            Dictionary<string, Instrument> instTp = curInstrLookup;// new Dictionary<string, Instrument>();
            Dictionary<string, InstrumentPair> pairsTp = curPairsLookup;// new Dictionary<string, InstrumentPair>();

            IEnumerable<Instrument> instResult;// = instTp.Values.ToList();
            IEnumerable<InstrumentPair> pairsResult;// = pairsTp.Values.ToList();

            if (shouldFetchFromAPI)
            {
                //fetch products...
                Console.WriteLine("fetching form API...");
                WebCallResult<IEnumerable<BinanceProduct>> products = await _client.General.GetProductsAsync();

                foreach (BinanceProduct p in products.Data)
                {

                    if (!instTp.ContainsKey(p.BaseAsset))
                    {
                        instTp[p.BaseAsset] = new Instrument
                        {
                            Symbol = p.BaseAsset,
                            SymbolChar = p.BaseAssetChar,
                            SymbolName = p.BaseAssetName,
                            Type = "",//ignore for now
                        };
                    }
                    if (!instTp.ContainsKey(p.QuoteAsset))
                    {
                        instTp[p.QuoteAsset] = new Instrument
                        {
                            Symbol = p.QuoteAsset,
                            SymbolChar = p.QuoteAssetChar,
                            SymbolName = p.QuoteAssetName,
                            Type = "",//ignore for now
                        };
                    }
                    if (!pairsTp.ContainsKey(p.Symbol))
                    {
                        pairsTp[p.Symbol] = new InstrumentPair()
                        {
                            Symbol = p.Symbol,
                            BaseInstrument = instTp[p.BaseAsset],
                            QuoteInstrument = instTp[p.QuoteAsset],
                        };
                    }
                }


                //fetch exhange info



                //List<> 
                WebCallResult<BinanceExchangeInfo> exchangeInfo = await _client.Spot.System.GetExchangeInfoAsync();
                List<BinanceSymbol> symbols = exchangeInfo.Data.Symbols.ToList();
                Dictionary<string, BinanceSymbol> symbolLookup = symbols.ToDictionary(s => s.Name);

                //update pairs info from fetched data

                foreach (InstrumentPair p in pairsTp.Values) {
                    BinanceSymbol b;
                    if (symbolLookup.TryGetValue(p.Symbol, out b)) { //found 
                        p.IceBergAllowed = b.IceBergAllowed;
                        p.IsMarginTradingAllowed = b.IsMarginTradingAllowed;
                        p.IsSpotTradingAllowed = b.IsSpotTradingAllowed;
                        p.OcoAllowed = b.OCOAllowed;
                        p.QuoteCommissionPrecision = b.QuoteCommissionPrecision;
                        p.QuoteOrderQuantityMarketAllowed = b.QuoteOrderQuantityMarketAllowed;
                        p.BaseCommissionPrecision = b.BaseCommissionPrecision;
                        //missing
                        //b.BaseAssetPrecision
                        //b.status ? 

                        //b.Filters
                        //+ othert filters .... b.IceBergPartsFilter

                    }
                    else {//not found 
                        //??
                    }

                }



                ////Do DB stuff
                instResult = instTp.Values.ToList();
                pairsResult = pairsTp.Values.ToList();

                //if (_context.Instrument.Count() == 0)
                //{
                //    //_context.Instrument.AddRange(instResult);//.Where(instruments => );// .AddRange(instruments).Where();//use async + upsert
                //    //_context.SaveChanges();
                //}

                //if (_context.InstrumentPair.Count() == 0)//???
                //{
                _context.InstrumentPair.AddRange(pairsResult);//.Where(instruments => );// .AddRange(instruments).Where();//use async + upsert
                //                                                  //_context.SaveChanges();
                //}

                _context.SaveChanges();
            }
            else {
                //use data from DB:
                instResult = instTp.Values.ToList();
                pairsResult = pairsTp.Values.ToList();

            }
            return pairsResult;
        }


        // GET: api/People
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetPerson()
        {
            return await _context.Person.ToListAsync();
        }

        // GET: api/People/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            var person = await _context.Person.FindAsync(id);

            if (person == null)
            {
                return NotFound();
            }

            return person;
        }

        // PUT: api/People/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, Person person)
        {
            if (id != person.Id)
            {
                return BadRequest();
            }

            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/People
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            _context.Person.Add(person);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPerson", new { id = person.Id }, person);
        }

        // DELETE: api/People/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            var person = await _context.Person.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            _context.Person.Remove(person);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.Id == id);
        }
    }
}
