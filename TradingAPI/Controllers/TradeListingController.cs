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
using CryptoExchange.Net.Authentication;
using Binance.Net.Enums;
using System.Collections.Concurrent;
using TradingAPI.Models;
using TradingAPI.Utilites;

namespace TradingAPI.Controllers
{
    /*
    Mostly just REST API endpoints here for testing
    with the exception of PopulateDbTablesFromApi, which
    initializes the instrument symbols.
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


        //[HttpGet("allHistory")]
        //public async Task<object> GetAllHistoryAsync()
        //{ // TODO clean-up

        //    var pairs_all = _context.InstrumentPair.ToList();
        //    var pairs = pairs_all.GetRange(100, 200);//4 testing


        //    int remCount = pairs.Count;//count async tasks

        //    ConcurrentBag<List<PriceHistory>> concurrentHistory = new ConcurrentBag<List<PriceHistory>>();
        //    List<List<PriceHistory>> allHistory = new List<List<PriceHistory>>();

        //    Task<object> resultTask = new Task<object>(() =>
        //    {
        //        Console.WriteLine("running return task ");
        //        return new { status = "fetching data async" };
        //    });

        //    Action<List<PriceHistory>> onCompleted = (List<PriceHistory> result) =>
        //    {
        //        remCount--;

        //        if (remCount <= 0)
        //        { //all tasks have completed

        //            var allHistoryFlat = new List<PriceHistory>();
        //            foreach (var p in allHistory)
        //            {
        //                allHistoryFlat.AddRange(p);
        //            }

        //            Console.WriteLine("all tasks complete with count: " + allHistoryFlat.Count);


        //            _context.PriceHistory.AddRange(allHistoryFlat);
        //            _context.SaveChanges();

        //            resultTask.RunSynchronously();

        //        }

        //    };

        //    Action<Task<List<PriceHistory>>> hasCompleted = async (PriceHistoryTp) =>
        //    {

        //        PriceHistoryTp.Wait();
        //        List<PriceHistory> historyTp = PriceHistoryTp.Result;
        //        bool gotLock = false;
        //        lock (allHistory)
        //        {
        //            allHistory.Add(historyTp);
        //            gotLock = true;
        //        }
        //        if (!gotLock)
        //            Console.WriteLine("<-----missed lock" + historyTp.Count);

        //        Console.WriteLine("completed task with len:" + historyTp.Count);

        //        onCompleted(historyTp);
        //    };


        //    foreach (var pair in pairs)
        //    {
        //        string symbol = pair.Symbol;

        //        var curHistoryQuery = _context.PriceHistory.Where(p => p.InstrumentPairId == symbol && p.Interval == KlineInterval.OneDay);
        //        var curHistory = curHistoryQuery.ToDictionary(p => p.UtcOpenTime);
        //        var curHistoryList = curHistoryQuery.ToList();
        //        InstrumentPair instrPair = _context.InstrumentPair.Where(p => p.Symbol == symbol).First();

        //        Console.WriteLine("getting history for " + pair.Symbol);
        //        curHistoryList.Sort((a, b) => a.UtcCloseTime.CompareTo(b.UtcCloseTime));//asc order (oldest first)

        //        if (pair.Symbol == "ACMBTC")
        //        {
        //            Console.WriteLine("<--------ACMBTC");
        //        }

        //        DateTime? lastHistoryTimeClose = null;
        //        DateTime? lastHistoryTimeCloseNext = null;
        //        if (curHistoryList.Count != 0)
        //        {
        //            var lastHistory = curHistoryList.Last();

        //            var lastDateTime = DateTime.UnixEpoch.AddMilliseconds((double)lastHistory.UtcCloseTime);
        //            lastHistoryTimeClose = lastDateTime;
        //            lastHistoryTimeCloseNext = lastDateTime.AddMilliseconds((double)TradingDbUtilies.intervalLookup[KlineInterval.OneDay]);
        //        }

        //        DateTime epoch = DateTime.UnixEpoch;
        //        DateTime now = DateTime.Now;
        //        long nowMs = (long)(now.Subtract(epoch)).TotalSeconds;


        //        if (lastHistoryTimeCloseNext != null && lastHistoryTimeCloseNext > DateTime.Now)
        //        {
        //            Console.WriteLine("pair " + pair.Symbol + " is already up to date");
        //            onCompleted(new List<PriceHistory>());//consider task completed
        //        }
        //        else
        //        {
        //            TradingDbUtilies.GetPriceHistoryAsync(
        //                KlineInterval.OneDay,
        //                _client,
        //                curHistory,
        //                instrPair,
        //                lastHistoryTimeClose
        //            ).ContinueWith(hasCompleted);
        //        }
        //        //GetHistory(pair.Symbol);//save to db
        //    }


        //    return resultTask;
        //}


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


        //public async Task PopulateInstrumentTables(IEnumerable<BinanceProduct> products) // Only add whats missing
        //{

        //    Dictionary<string, Instrument> curInstrLookup = _context.Instrument.ToDictionary(i => i.Symbol);
        //    Dictionary<string, Instrument> instTp = curInstrLookup;// new Dictionary<string, Instrument>();

        //    List<Instrument> instruments = new List<Instrument>();
        //    foreach (BinanceProduct p in products)
        //    {
        //        if (!instTp.ContainsKey(p.BaseAsset))
        //        {
        //            var instrTp = new Instrument
        //            {
        //                Symbol = p.BaseAsset,
        //                SymbolChar = p.BaseAssetChar,
        //                Name = p.BaseAssetName,
        //                Type = "",//ignore for now
        //            };
        //            instTp[p.BaseAsset] = instrTp;//mark as taken
        //            instruments.Add(instrTp);
        //        }
        //        if (!instTp.ContainsKey(p.QuoteAsset))
        //        {
        //            var instrTp = new Instrument
        //            {
        //                Symbol = p.QuoteAsset,
        //                SymbolChar = p.QuoteAssetChar,
        //                Name = p.QuoteAssetName,
        //                Type = "",//ignore for now
        //            };
        //            instTp[p.QuoteAsset] = instrTp;//mark as taken
        //            instruments.Add(instrTp);
        //        }
        //    }
        //    //Save to DB
        //    _context.Instrument.AddRange(instruments);
        //    _context.SaveChanges();
        //}

        //public async Task PopulateInstrumentPairsTables(IEnumerable<BinanceProduct> products) // Only add whats missing
        //{
        //    Dictionary<string, Instrument> curInstrLookup = _context.Instrument.ToDictionary(i => i.Symbol);
        //    Dictionary<string, Instrument> curInstru = curInstrLookup;// new Dictionary<string, Instrument>();

        //    Dictionary<string, InstrumentPair> curPairsLookup = _context.InstrumentPair.ToDictionary(i => i.Symbol);
        //    Dictionary<string, InstrumentPair> curPairs = curPairsLookup;// new Dictionary<string, InstrumentPair>();

        //    List<Instrument> instruments = new List<Instrument>();
        //    List<InstrumentPair> pairs = new List<InstrumentPair>();

        //    foreach (BinanceProduct p in products)
        //    {
        //        if (!curInstru.ContainsKey(p.BaseAsset))
        //        {
        //            var instrTp = new Instrument
        //            {
        //                Symbol = p.BaseAsset,
        //                SymbolChar = p.BaseAssetChar,
        //                Name = p.BaseAssetName,
        //                Type = "",//ignore for now
        //            };
        //            curInstru[p.BaseAsset] = instrTp;//mark as taken
        //            instruments.Add(instrTp);
        //        }
        //        if (!curInstru.ContainsKey(p.QuoteAsset))
        //        {
        //            var instrTp = new Instrument
        //            {
        //                Symbol = p.QuoteAsset,
        //                SymbolChar = p.QuoteAssetChar,
        //                Name = p.QuoteAssetName,
        //                Type = "",//ignore for now
        //            };
        //            curInstru[p.QuoteAsset] = instrTp;//mark as taken
        //            instruments.Add(instrTp);
        //        }

        //        if (!curPairs.ContainsKey(p.Symbol))
        //        {
        //            var pairTp = new InstrumentPair()
        //            {
        //                Symbol = p.Symbol,
        //                BaseInstrument = curInstru[p.BaseAsset],
        //                QuoteInstrument = curInstru[p.QuoteAsset],
        //            };
        //            curPairs[p.Symbol] = pairTp;//mark as taken
        //            pairs.Add(pairTp);
        //        }
        //    }

        //    //Save to DB
        //    _context.InstrumentPair.AddRange(pairs);

        //    _context.SaveChanges();
        //}


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
