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
