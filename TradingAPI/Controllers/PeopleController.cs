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

namespace TradingAPI.Controllers
{

    

    public class TradingDbUtilies {

        static int minute = 60000;//ms
        static int hour = minute * 60;//ms
        static int day = hour * 24;//ms
        static int week = day * 7;//ms

        public static Dictionary<KlineInterval, long> intervalLookup = new Dictionary<KlineInterval, long>() {
            { KlineInterval.OneMinute, minute },
            { KlineInterval.ThreeMinutes, 3 * minute },
            { KlineInterval.FiveMinutes, minute * 5 },
            { KlineInterval.FifteenMinutes, minute * 15 },
            { KlineInterval.ThirtyMinutes, 30 * minute },

            { KlineInterval.OneHour, hour },
            { KlineInterval.TwoHour, 2 * hour },
            { KlineInterval.FourHour, 4 * hour },
            { KlineInterval.SixHour, 6 * hour },
            { KlineInterval.EightHour, 8 * hour },
            { KlineInterval.TwelveHour, 12 * hour },

            { KlineInterval.OneDay, day },
            { KlineInterval.ThreeDay, 3 * day },            
            
            { KlineInterval.OneWeek, week },
            //{ KlineInterval.OneMonth, 0 },//not mapped           
        };

        public static async Task<List<PriceHistory>> GetPriceHistoryAsync(
            KlineInterval interval, 
            BinanceClient binanceClient, 
            Dictionary<long, PriceHistory> currentHistory, 
            InstrumentPair instrumentPair,
            DateTime? startTime = null
        )
        { //requires save
            //var intervals = new[] {//limit to 500 records back
            //    KlineInterval.OneDay,
            //    //KlineInterval.OneHour
            //};

            string symbol = instrumentPair.Symbol;

            //WebCallResult<IEnumerable<IBinanceRecentTrade>> tickers = await _client.Spot.Market.GetTradeHistoryAsync(symbol); //
            WebCallResult<IEnumerable<IBinanceKline>> klines = await binanceClient.Spot.Market.GetKlinesAsync(symbol,
                interval,//Per day
                startTime//startTime
            ); //

            //DateTime lastCloseTime = tickers.Data.Last().CloseTime;

            //tickers

            //var curHistory = _context.PriceHistory.Where(p => p.InstrumentPairId == symbol && p.Interval == KlineInterval.OneDay).ToDictionary(p => p.UtcOpenTime);
            var newHistory = new List<PriceHistory>();

            //InstrumentPair instrPair = _context.InstrumentPair.Where(p => p.Symbol == symbol).First();

            foreach (var k in klines.Data)
            {
                long curTime = (long)k.OpenTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
                if (!currentHistory.ContainsKey(curTime))
                { //no existing record
                    long curCloseTime = (long)k.CloseTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
                    var historyTp = new PriceHistory
                    {
                        InstrumentPair = instrumentPair,
                        Interval = interval,
                        UtcOpenTime = curTime,
                        UtcCloseTime = curCloseTime,
                        Open = k.Open,
                        Close = k.Close,
                        High = k.High,
                        Low = k.Low,
                        TradeCount = k.TradeCount,
                    };
                    currentHistory[curTime] = historyTp;//mark as taken
                    newHistory.Add(historyTp);
                }
            }

            Console.WriteLine("got history Async for " + instrumentPair.Symbol);

            return newHistory;

            //Save to DB
            //_context.PriceHistory.AddRange(newHistory);
            //_context.SaveChanges();            
        }
    }



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
                ApiCredentials = new ApiCredentials(
                    "WApQfvJsMkriy3TmWqckeJo1z50pxpCGWj6dC1gQ1PhtBLwXkkxIClotV1T1q2W3", //key
                    "rNZbarFqYkTve6RSiWgcsZNPHqg09f8jcd7zA7q6D88VQMJahgXJheVxtaYWJMHT" //secret
                )//TODO: move to config
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

        [HttpGet("tickers")]
        public async Task<IEnumerable<IBinanceTick>> GetTickers()
        {
            WebCallResult<IEnumerable<IBinanceTick>> tickers = await _client.Spot.Market.GetTickersAsync(); //
            return tickers.Data;
        }

        [HttpGet("historyCount/{symbol}")]
        public async Task<object> GetPriceHistory(string symbol) {
            DateTime lastCloseTime = DateTime.UnixEpoch;
            DateTime now = DateTime.Now;
            List<IBinanceKline> all = new List<IBinanceKline>();

            while (lastCloseTime.CompareTo(now) < 0) {
                WebCallResult<IEnumerable<IBinanceKline>> tickers = await _client.Spot.Market.GetKlinesAsync(symbol,
                    KlineInterval.OneDay,
                    lastCloseTime,
                    DateTime.UtcNow,
                    1000
                ); //

                //if (tickers.Success)
                //{
                if (tickers.Data != null && tickers.Data.Count() != 0)
                    all.AddRange(tickers.Data);
                else
                    break;
                //}
                //else break;

                lastCloseTime = tickers.Data.Last().CloseTime;
            }

            var lastTime = all.Last();
            
            return new { count = all.Count };
        }

        [HttpGet("allHistory")]
        public async Task<object> GetAllHistoryAsync() { // TODO clean-up


            var pairs_all = _context.InstrumentPair.ToList();
            var pairs = pairs_all.GetRange(100, 200);//4 testing


            int remCount = pairs.Count;//count async tasks

            ConcurrentBag<List<PriceHistory>> concurrentHistory = new ConcurrentBag<List<PriceHistory>>();
            List<List<PriceHistory>> allHistory = new List<List<PriceHistory>>();

            Task<object> resultTask = new Task<object>(() => {
                Console.WriteLine("running return task ");
                return new { status = "fetching data async" };
            });

            Action<List<PriceHistory>> onCompleted = (List<PriceHistory> result) =>
            {
                remCount--;               

                if (remCount <= 0)
                { //all tasks have completed

                    //// Consume the items in the bag
                    //List<Task> bagConsumeTasks = new List<Task>();
                    //List<PriceHistory> allHistory = new List<PriceHistory>();
                    ////int itemsInBag = 0;
                    //while (!concurrentHistory.IsEmpty)
                    //{
                    //    bagConsumeTasks.Add(Task.Run(() =>
                    //    {
                    //        List<PriceHistory> item;
                    //        lock (allHistory)
                    //            if (concurrentHistory.TryTake(out item))
                    //            {
                    //                //Console.WriteLine(item);
                    //                //itemsInBag++;
                                
                    //                allHistory.AddRange(item);
                    //            }
                    //    }));
                    //}
                    //Task.WaitAll(bagConsumeTasks.ToArray());




                    
                    //onCompleted(allHistory);
                    //_context.PriceHistory.AddRange(allHistory);
                    //_context.SaveChanges();

                    var allHistoryFlat = new List<PriceHistory>();
                    foreach (var p in allHistory) {
                        allHistoryFlat.AddRange(p);
                    }

                    Console.WriteLine("all tasks complete with count: " + allHistoryFlat.Count);


                    _context.PriceHistory.AddRange(allHistoryFlat);
                    _context.SaveChanges();

                    resultTask.RunSynchronously();

                }

            };


            
            //List<Task> runSync = new List<Task>();

            Action<Task<List<PriceHistory>>> hasCompleted = async (PriceHistoryTp) => {

                PriceHistoryTp.Wait();
                List<PriceHistory> historyTp = PriceHistoryTp.Result;
                bool gotLock = false;
                lock (allHistory)
                {
                    allHistory.Add(historyTp);
                    gotLock = true;
                }
                if(!gotLock)
                    Console.WriteLine("<-----missed lock" + historyTp.Count);

                //concurrentHistory.Add(historyTp);
                 //_context.PriceHistory.AddRange(historyTp);

                //remCount--;
                Console.WriteLine("completed task with len:" + historyTp.Count);

                 onCompleted(historyTp);
                 //if (remCount <= 0)
                 //{ //all tasks completed
                 //    Console.WriteLine("all tasks complete");
                 //    onCompleted(allHistory);
                 //    //_context.PriceHistory.AddRange(allHistory);
                 //    //_context.SaveChanges();
                     
                 //}
            };


            foreach (var pair in pairs) {
                string symbol = pair.Symbol;
                //GetPriceHistoryAsync().ContinueWith(hasCompleted);
                var curHistoryQuery = _context.PriceHistory.Where(p => p.InstrumentPairId == symbol && p.Interval == KlineInterval.OneDay);
                var curHistory = curHistoryQuery.ToDictionary(p => p.UtcOpenTime);
                var curHistoryList = curHistoryQuery.ToList();
                InstrumentPair instrPair = _context.InstrumentPair.Where(p => p.Symbol == symbol).First();
                //List<PriceHistory> newHistory = 
                Console.WriteLine("getting history for " + pair.Symbol);
                curHistoryList.Sort((a, b) => a.UtcCloseTime.CompareTo(b.UtcCloseTime));//asc order (oldest first)

                if (pair.Symbol == "ACMBTC") {
                    Console.WriteLine("<--------ACMBTC");
                }
                
                DateTime? lastHistoryTimeClose = null;
                DateTime? lastHistoryTimeCloseNext = null;
                if (curHistoryList.Count != 0) {
                    var lastHistory = curHistoryList.Last();

                    //double utcTime = (double)lastHistory.UtcCloseTime
                    var lastDateTime = DateTime.UnixEpoch.AddMilliseconds((double)lastHistory.UtcCloseTime);
                    lastHistoryTimeClose = lastDateTime;
                    lastHistoryTimeCloseNext = lastDateTime.AddMilliseconds((double)TradingDbUtilies.intervalLookup[KlineInterval.OneDay]);
                }

                DateTime epoch = DateTime.UnixEpoch;
                DateTime now = DateTime.Now;
                long nowMs = (long)(now.Subtract(epoch)).TotalSeconds;


                //if (lastHistory.UtcOpenTime + TradingDbUtilies.intervalLookup[KlineInterval.OneDay] > nowMs)


                if (lastHistoryTimeCloseNext != null && lastHistoryTimeCloseNext > DateTime.Now)
                //{//lastHistory.UtcCloseTime > nowMs)
                {
                    Console.WriteLine("pair " + pair.Symbol + " is already up to date");
                    onCompleted(new List<PriceHistory>());//consider task completed
                }
                else
                {
                    TradingDbUtilies.GetPriceHistoryAsync(
                        KlineInterval.OneDay, 
                        _client, 
                        curHistory, 
                        instrPair,
                        lastHistoryTimeClose
                    ).ContinueWith(hasCompleted);
                }
                //GetHistory(pair.Symbol);//save to db
            }


            return resultTask;
            /*

            var curHistory = _context.PriceHistory.Where(p => p.InstrumentPairId == symbol && p.Interval == KlineInterval.OneDay).ToDictionary(p => p.UtcOpenTime);
            InstrumentPair instrPair = _context.InstrumentPair.Where(p => p.Symbol == symbol).First();

            List<PriceHistory> newHistory = await TradingDbUtilies.GetPriceHistoryAsync(_client, curHistory, instrPair);

            //Save to DB
            _context.PriceHistory.AddRange(newHistory);
            _context.SaveChanges();

            return new { status = "got price history, records added: " + newHistory.Count };

            */



            //return new { status = "fetching data async" };
        }





        //public async Task<List<PriceHistory>> GetPriceHistoryAsync(string symbol, Dictionary<long, PriceHistory> currentHistory, InstrumentPair instrumentPair) { //requires save
        //    var intervals = new[] {//limit to 500 records back
        //        KlineInterval.OneDay,
        //        //KlineInterval.OneHour
        //    };

        //    //WebCallResult<IEnumerable<IBinanceRecentTrade>> tickers = await _client.Spot.Market.GetTradeHistoryAsync(symbol); //
        //    WebCallResult<IEnumerable<IBinanceKline>> tickers = await _client.Spot.Market.GetKlinesAsync(symbol,
        //        intervals[0]//Per day
        //    ); //

        //    //DateTime lastCloseTime = tickers.Data.Last().CloseTime;

        //    //tickers

        //    //var curHistory = _context.PriceHistory.Where(p => p.InstrumentPairId == symbol && p.Interval == KlineInterval.OneDay).ToDictionary(p => p.UtcOpenTime);
        //    var newHistory = new List<PriceHistory>();

        //    //InstrumentPair instrPair = _context.InstrumentPair.Where(p => p.Symbol == symbol).First();

        //    foreach (var k in tickers.Data)
        //    {
        //        long curTime = (long)k.OpenTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
        //        if (!currentHistory.ContainsKey(curTime))
        //        { //no existing record
        //            long curCloseTime = (long)k.CloseTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
        //            var historyTp = new PriceHistory
        //            {
        //                InstrumentPair = instrumentPair,
        //                Interval = KlineInterval.OneDay,
        //                UtcOpenTime = curTime,
        //                UtcCloseTime = curCloseTime,
        //                Open = k.Open,
        //                Close = k.Close,
        //                High = k.High,
        //                Low = k.Low,
        //                TradeCount = k.TradeCount,
        //            };
        //            currentHistory[curTime] = historyTp;//mark as taken
        //            newHistory.Add(historyTp);
        //        }
        //    }

        //    return newHistory;

        //    //Save to DB
        //    //_context.PriceHistory.AddRange(newHistory);
        //    //_context.SaveChanges();            
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

        public async Task<IEnumerable<IBinanceKline>> GetHistory_prev(string symbol)
        {
            var intervals = new[] {//limit to 500 records back
                KlineInterval.OneDay,
                //KlineInterval.OneHour
            };

            //WebCallResult<IEnumerable<IBinanceRecentTrade>> tickers = await _client.Spot.Market.GetTradeHistoryAsync(symbol); //
            WebCallResult<IEnumerable<IBinanceKline>> tickers = await _client.Spot.Market.GetKlinesAsync(symbol,
                intervals[0]//Per day
            ); //

            //DateTime lastCloseTime = tickers.Data.Last().CloseTime;

            //tickers

            var curHistory = _context.PriceHistory.Where(p => p.InstrumentPairId == symbol && p.Interval == KlineInterval.OneDay).ToDictionary(p => p.UtcOpenTime);
            var newHistory = new List<PriceHistory>();

            InstrumentPair instrPair = _context.InstrumentPair.Where(p => p.Symbol == symbol).First();

            foreach (var k in tickers.Data) {
                long curTime = (long)k.OpenTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
                if (!curHistory.ContainsKey(curTime)) { //no existing record
                    long curCloseTime = (long)k.CloseTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
                    var historyTp = new PriceHistory {
                        InstrumentPair = instrPair,
                        Interval = KlineInterval.OneDay,
                        UtcOpenTime = curTime,
                        UtcCloseTime = curCloseTime,
                        Open = k.Open,
                        Close = k.Close,
                        High = k.High,
                        Low = k.Low,
                        TradeCount = k.TradeCount,
                    };
                    curHistory[curTime] = historyTp;//mark as taken
                    newHistory.Add(historyTp);
                }
            }

            //Save to DB
            _context.PriceHistory.AddRange(newHistory);
            _context.SaveChanges();


            //Test records...
            var curHistoryTest = _context.PriceHistory.Where(p => p.InstrumentPairId == symbol && p.Interval == KlineInterval.OneDay).ToList();

            //WebCallResult<IEnumerable<IBinanceKline>> tickers2 = await _client.Spot.Market.GetKlinesAsync(symbol,
            //    KlineInterval.FifteenMinutes,
            //    lastCloseTime,
            //    DateTime.UtcNow,
            //    1000
            //); //
            //_client.Spot.Market.GetKlinesAsync()

            //List<IBinanceKline> all = new List<IBinanceKline>();
            //all.AddRange(tickers.Data);
            //all.AddRange(tickers2.Data);
            //var all = tickers.Data.ToList().AddRange(tickers2.Data.ToList());
            //var all2 = new List<IBinanceKline>()
            return tickers.Data;
            //return curHistoryTest.ToList();
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


        public async Task PopulateInstrumentTables(IEnumerable<BinanceProduct> products) // Only add whats missing
        {
            //WebCallResult<IEnumerable<BinanceProduct>> products = await _client.General.GetProductsAsync();

            //_client.Spot.Market.Get

            Dictionary<string, Instrument> curInstrLookup = _context.Instrument.ToDictionary(i => i.Symbol);
            Dictionary<string, Instrument> instTp = curInstrLookup;// new Dictionary<string, Instrument>();

            List<Instrument> instruments = new List<Instrument>();
            foreach (BinanceProduct p in products)
            {
                if (!instTp.ContainsKey(p.BaseAsset))
                {
                    var instrTp = new Instrument
                    {
                        Symbol = p.BaseAsset,
                        SymbolChar = p.BaseAssetChar,
                        Name = p.BaseAssetName,
                        Type = "",//ignore for now
                    };
                    instTp[p.BaseAsset] = instrTp;//mark as taken
                    instruments.Add(instrTp);
                }
                if (!instTp.ContainsKey(p.QuoteAsset))
                {
                    var instrTp = new Instrument
                    {
                        Symbol = p.QuoteAsset,
                        SymbolChar = p.QuoteAssetChar,
                        Name = p.QuoteAssetName,
                        Type = "",//ignore for now
                    };
                    instTp[p.QuoteAsset] = instrTp;//mark as taken
                    instruments.Add(instrTp);
                }
            }
            //Save to DB
            _context.Instrument.AddRange(instruments);
            _context.SaveChanges();
        }

        public async Task PopulateInstrumentPairsTables(IEnumerable<BinanceProduct> products) // Only add whats missing
        {
            //WebCallResult<IEnumerable<BinanceProduct>> products = await _client.General.GetProductsAsync();

            Dictionary<string, Instrument> curInstrLookup = _context.Instrument.ToDictionary(i => i.Symbol);
            Dictionary<string, Instrument> curInstru = curInstrLookup;// new Dictionary<string, Instrument>();

            Dictionary<string, InstrumentPair> curPairsLookup = _context.InstrumentPair.ToDictionary(i => i.Symbol);
            Dictionary<string, InstrumentPair> curPairs = curPairsLookup;// new Dictionary<string, InstrumentPair>();

            List<Instrument> instruments = new List<Instrument>();
            List<InstrumentPair> pairs = new List<InstrumentPair>();

            foreach (BinanceProduct p in products)
            {
                if (!curInstru.ContainsKey(p.BaseAsset))
                {
                    var instrTp = new Instrument
                    {
                        Symbol = p.BaseAsset,
                        SymbolChar = p.BaseAssetChar,
                        Name = p.BaseAssetName,
                        Type = "",//ignore for now
                    };
                    curInstru[p.BaseAsset] = instrTp;//mark as taken
                    instruments.Add(instrTp);
                }
                if (!curInstru.ContainsKey(p.QuoteAsset))
                {
                    var instrTp = new Instrument
                    {
                        Symbol = p.QuoteAsset,
                        SymbolChar = p.QuoteAssetChar,
                        Name = p.QuoteAssetName,
                        Type = "",//ignore for now
                    };
                    curInstru[p.QuoteAsset] = instrTp;//mark as taken
                    instruments.Add(instrTp);
                }

                if (!curPairs.ContainsKey(p.Symbol))
                {
                    var pairTp = new InstrumentPair()
                    {
                        Symbol = p.Symbol,
                        BaseInstrument = curInstru[p.BaseAsset],
                        QuoteInstrument = curInstru[p.QuoteAsset],
                    };
                    curPairs[p.Symbol] = pairTp;//mark as taken
                    pairs.Add(pairTp);
                }
            }

            //Save to DB
            //_context.Instrument.AddRange(instruments);
            _context.InstrumentPair.AddRange(pairs);
            
            _context.SaveChanges();
        }


        [HttpGet("test")]
        public async Task<object> Test()//test
        {
            WebCallResult<IEnumerable<BinanceProduct>> products = await _client.General.GetProductsAsync();
            //await PopulateInstrumentTables(products.Data);
            await PopulateInstrumentPairsTables(products.Data);
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
