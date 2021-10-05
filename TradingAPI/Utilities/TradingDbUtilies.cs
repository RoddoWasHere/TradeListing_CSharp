using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects;
using Binance.Net.Objects.Other;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingAPI.Data;
using TradingAPI.Models;

namespace TradingAPI.Utilites
{
    public class TradingDbUtilies
    {
        public static BinanceClient _client = new BinanceClient(new BinanceClientOptions()
        {
            ApiCredentials = new ApiCredentials(
                    "WApQfvJsMkriy3TmWqckeJo1z50pxpCGWj6dC1gQ1PhtBLwXkkxIClotV1T1q2W3", //key
                    "rNZbarFqYkTve6RSiWgcsZNPHqg09f8jcd7zA7q6D88VQMJahgXJheVxtaYWJMHT" //secret
                )//TODO: move to config
                 // Specify options for the client
        });

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
        {
            string symbol = instrumentPair.Symbol;

            WebCallResult<IEnumerable<IBinanceKline>> klines = await binanceClient.Spot.Market.GetKlinesAsync(symbol,
                interval,//Per day
                startTime//startTime
            ); //

            var newHistory = new List<PriceHistory>();

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

        /// <summary>
        /// Sends all the API requests asynchronously and waits for all the responses.        /// 
        /// </summary>
        /// <param name="_context"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static async Task<List<PriceHistory>> GetHistoryBatchAsync(
            TradeListingDbContext _context, 
            KlineInterval klineInterval, 
            int start, 
            int length
        )
        { // TODO clean-up

            var pairs_all = _context.InstrumentPair.ToList();//.Sort((a, b) => a.Symbol.CompareTo(b.Symbol));
            var end = length > pairs_all.Count - start ? pairs_all.Count - start : length;
            var pairs = pairs_all.GetRange(start, end);//4 testing


            int remCount = pairs.Count;//count async tasks

            ConcurrentBag<List<PriceHistory>> concurrentHistory = new ConcurrentBag<List<PriceHistory>>();
            List<List<PriceHistory>> allHistory = new List<List<PriceHistory>>();

            var allHistoryFlat = new List<PriceHistory>();

            Task resultTask = new Task(() =>
            {
                Console.WriteLine("running return task ");
            });
            

            Action<List<PriceHistory>> onCompleted = (List<PriceHistory> result) =>
            {
                remCount--;

                if (remCount <= 0)
                { //all tasks have completed                    
                    foreach (var p in allHistory)
                    {
                        allHistoryFlat.AddRange(p);
                    }

                    Console.WriteLine("all tasks complete with count: " + allHistoryFlat.Count);


                    _context.PriceHistory.AddRange(allHistoryFlat);
                    _context.SaveChanges();

                    resultTask.RunSynchronously();
                }
            };

            Action<Task<List<PriceHistory>>> hasCompleted = async (PriceHistoryTp) =>
            {

                PriceHistoryTp.Wait();
                List<PriceHistory> historyTp = PriceHistoryTp.Result;
                bool gotLock = false;
                lock (allHistory)
                {
                    allHistory.Add(historyTp);
                    gotLock = true;
                }
                if (!gotLock)
                    Console.WriteLine("<-----missed lock" + historyTp.Count);

                Console.WriteLine("completed task with len:" + historyTp.Count);

                onCompleted(historyTp);
            };


            foreach (var pair in pairs)
            {
                string symbol = pair.Symbol;

                var curHistoryQuery = _context.PriceHistory.Where(p => p.InstrumentPairId == symbol && p.Interval == klineInterval);
                var curHistory = curHistoryQuery.ToDictionary(p => p.UtcOpenTime);
                var curHistoryList = curHistoryQuery.ToList();
                InstrumentPair instrPair = _context.InstrumentPair.Where(p => p.Symbol == symbol).First();

                Console.WriteLine("getting history for " + pair.Symbol);
                curHistoryList.Sort((a, b) => a.UtcCloseTime.CompareTo(b.UtcCloseTime));//asc order (oldest first)

                //if (pair.Symbol == "ACMBTC")
                //{
                //    Console.WriteLine("<--------ACMBTC");
                //}

                DateTime? lastHistoryTimeClose = null;
                DateTime? lastHistoryTimeCloseNext = null;
                if (curHistoryList.Count != 0)
                {
                    var lastHistory = curHistoryList.Last();

                    var lastDateTime = DateTime.UnixEpoch.AddMilliseconds((double)lastHistory.UtcCloseTime);
                    lastHistoryTimeClose = lastDateTime;
                    lastHistoryTimeCloseNext = lastDateTime.AddMilliseconds((double)TradingDbUtilies.intervalLookup[klineInterval]);
                }

                DateTime epoch = DateTime.UnixEpoch;
                DateTime now = DateTime.Now;
                long nowMs = (long)(now.Subtract(epoch)).TotalSeconds;


                if (lastHistoryTimeCloseNext != null && lastHistoryTimeCloseNext > DateTime.Now)
                {
                    //Console.WriteLine("pair " + pair.Symbol + " is already up to date");
                    onCompleted(new List<PriceHistory>());//consider task completed
                }
                else
                {
                    //send all requests qithout waiting for the results
                    TradingDbUtilies.GetPriceHistoryAsync(
                        klineInterval,
                        _client,
                        curHistory,
                        instrPair,
                        lastHistoryTimeClose
                    ).ContinueWith(hasCompleted);
                }
                //GetHistory(pair.Symbol);//save to db
            }
            //Wait for all the requests to be returned
            resultTask.Wait();

            return allHistoryFlat;
        }


        public static async Task PopulateInstrumentPairsTables(TradeListingDbContext _context, IEnumerable<BinanceProduct> products) // Only add whats missing
        {
            Dictionary<string, Instrument> curInstru = _context.Instrument.ToDictionary(i => i.Symbol);

            Dictionary<string, InstrumentPair> curPairs = _context.InstrumentPair.ToDictionary(i => i.Symbol);

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
            _context.InstrumentPair.AddRange(pairs);

            _context.SaveChanges();
        }


    }
}