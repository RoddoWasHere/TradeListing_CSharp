using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradingAPI.Data;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using Binance.Net.Objects.Spot.MarketData;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Other;
using CryptoExchange.Net.Authentication;
using Binance.Net.Enums;
using TradingAPI.Models;
using TradingAPI.Utilites;

namespace TradingAPI.Controllers
{
    /*
    Just REST API endpoints here in this controller for testing    
    See Query.cs for Graph API used in the application 
    */
    [Route("")]
    [ApiController]
    public class TradeListingController : ControllerBase
    {
        private readonly TradeListingDbContext _context;

        private readonly BinanceClient _client;
        public TradeListingController(TradeListingDbContext context)
        {
            _context = context;

            _client = new BinanceClient(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(
                    "WApQfvJsMkriy3TmWqckeJo1z50pxpCGWj6dC1gQ1PhtBLwXkkxIClotV1T1q2W3", //key
                    "rNZbarFqYkTve6RSiWgcsZNPHqg09f8jcd7zA7q6D88VQMJahgXJheVxtaYWJMHT" //secret
                )//TODO: move to config
                // Specify options for the client
            });
        }


        [HttpGet("exchangeInfo")]
        public async Task<ActionResult<BinanceExchangeInfo>> GetExchangeInfo()
        {
            WebCallResult<BinanceExchangeInfo> exchangeInfo = await _client.Spot.System.GetExchangeInfoAsync();

            return exchangeInfo.Data;
        }

        [HttpGet("exchangeInfoSave")]
        public async Task<ActionResult<BinanceExchangeInfo>> GetExchangeInfoSave()
        {
            WebCallResult<BinanceExchangeInfo> exchangeInfo = await _client.Spot.System.GetExchangeInfoAsync();
            return exchangeInfo.Data;
        }


        [HttpGet("prices")]
        public async Task<IEnumerable<BinancePrice>> GetPrices()
        {
            WebCallResult<IEnumerable<BinancePrice>> prices = await _client.Spot.Market.GetPricesAsync(); //
            return prices.Data;
        }

        [HttpGet("tickers")]
        public async Task<IEnumerable<IBinanceTick>> GetTickers()
        {
            WebCallResult<IEnumerable<IBinanceTick>> tickers = await _client.Spot.Market.GetTickersAsync(); //
            return tickers.Data;
        }

        [HttpGet("historyCount/{symbol}")]
        public async Task<object> GetPriceHistory(string symbol)
        {
            DateTime lastCloseTime = DateTime.UnixEpoch;
            DateTime now = DateTime.Now;
            List<IBinanceKline> all = new List<IBinanceKline>();

            while (lastCloseTime.CompareTo(now) < 0)
            {
                WebCallResult<IEnumerable<IBinanceKline>> tickers = await _client.Spot.Market.GetKlinesAsync(symbol,
                    KlineInterval.OneDay,
                    lastCloseTime,
                    DateTime.UtcNow,
                    1000
                ); //

                if (tickers.Data != null && tickers.Data.Count() != 0)
                    all.AddRange(tickers.Data);
                else
                    break;

                lastCloseTime = tickers.Data.Last().CloseTime;
            }

            var lastTime = all.Last();

            return new { count = all.Count };
        }
        
        [HttpGet("history/{symbol}")]
        public async Task<object> GetHistory(string symbol)
        {

            var curHistory = _context.PriceHistory.Where(p => p.InstrumentPairId == symbol && p.Interval == KlineInterval.OneDay).ToDictionary(p => p.UtcOpenTime);
            InstrumentPair instrPair = _context.InstrumentPair.Where(p => p.Symbol == symbol).First();

            List<PriceHistory> newHistory = await TradingDbUtilies.GetPriceHistoryAsync(KlineInterval.OneDay, _client, curHistory, instrPair);

            //Save to DB
            _context.PriceHistory.AddRange(newHistory);
            _context.SaveChanges();

            return new { status = "got price history, records added: " + newHistory.Count };
        }
        
        [HttpGet("symbol/{symbol}")]
        public async Task<ActionResult<BinanceExchangeInfo>> GetSymbolInfo(string symbol)
        {
            WebCallResult<BinanceExchangeInfo> exchangeInfo = await _client.Spot.System.GetExchangeInfoAsync(symbol);

            return exchangeInfo.Data;
        }

        [HttpGet("coins")]
        public async Task<IEnumerable<BinanceProduct>> GetCoins()
        {
            WebCallResult<IEnumerable<BinanceProduct>> products = await _client.General.GetProductsAsync();

            return products.Data;
        }

        [HttpGet("test")]
        public async Task<object> Test()//test
        {
            WebCallResult<IEnumerable<BinanceProduct>> products = await _client.General.GetProductsAsync();
            await TradingDbUtilies.PopulateInstrumentPairsTables(_context, products.Data);
            return new
            {
                status = "No errors",
            };
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
                            Name = p.BaseAssetName,
                            Type = "",//ignore for now
                        };
                    }
                    if (!instTp.ContainsKey(p.QuoteAsset))
                    {
                        instTp[p.QuoteAsset] = new Instrument
                        {
                            Symbol = p.QuoteAsset,
                            SymbolChar = p.QuoteAssetChar,
                            Name = p.QuoteAssetName,
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

                foreach (InstrumentPair p in pairsTp.Values)
                {
                    BinanceSymbol b;
                    if (symbolLookup.TryGetValue(p.Symbol, out b))
                    { //found 
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
                    else
                    {//not found 
                        //??
                    }

                }

                ////Do DB stuff
                instResult = instTp.Values.ToList();
                pairsResult = pairsTp.Values.ToList();

                _context.InstrumentPair.AddRange(pairsResult);

                _context.SaveChanges();
            }
            else
            {
                //use data from DB:
                instResult = instTp.Values.ToList();
                pairsResult = pairsTp.Values.ToList();

            }
            return pairsResult;
        }
    }
}
