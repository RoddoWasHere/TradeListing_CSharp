using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using HotChocolate;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingAPI.Controllers;
using TradingAPI.Data;
using TradingAPI.Models;
using TradingAPI.Utilites;

namespace TradingAPI.Schema
{
    public class Query
    {
        private readonly BinanceClient _client;

        public Query()
        {
            _client = new BinanceClient(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(
                    "WApQfvJsMkriy3TmWqckeJo1z50pxpCGWj6dC1gQ1PhtBLwXkkxIClotV1T1q2W3", //key
                    "rNZbarFqYkTve6RSiWgcsZNPHqg09f8jcd7zA7q6D88VQMJahgXJheVxtaYWJMHT" //secret
                )//TODO: move to config
            });
        }


        async Task<object> FetchHistoryFromApi(string symbol, TradeListingDbContext _context, KlineInterval klineInterval = KlineInterval.OneDay)
        {

            var curHistory = _context.PriceHistory.Where(p => p.InstrumentPairId == symbol && p.Interval == klineInterval).ToDictionary(p => p.UtcOpenTime);
            InstrumentPair instrPair = _context.InstrumentPair.Where(p => p.Symbol == symbol).First();

            List<PriceHistory> newHistory = await TradingDbUtilies.GetPriceHistoryAsync(klineInterval, _client, curHistory, instrPair);

            Console.WriteLine("Got new history from api: " + newHistory.Count);

            //Save to DB
            _context.PriceHistory.AddRange(newHistory);
            _context.SaveChanges();



            return new { status = "got price history, records added: " + newHistory.Count };
        }

        [UseProjection]
        public IQueryable<Instrument> GetInstruments([Service] TradeListingDbContext _context) {
            return _context.Instrument;
        }

        [UseProjection]
        public IQueryable<InstrumentPair> GetInstrumentPairs([Service] TradeListingDbContext _context){
            return _context.InstrumentPair;
        }

        [UseProjection]
        public IQueryable<InstrumentPair> GetInstrumentPair([Service] TradeListingDbContext _context, string pairSymbol)
        {
            return _context.InstrumentPair.Where(i => i.Symbol == pairSymbol);
        }

        [UseProjection]
        public async Task<InstrumentPair> GetInstrumentPairHistory(
            [Service] TradeListingDbContext _context, 
            string pairSymbol, 
            long startUctTime, 
            long endUctTime = -1,
            KlineInterval klineInterval = KlineInterval.OneDay
        ){

            Console.WriteLine("---Getting InstrumentPairHistory for "+pairSymbol);

            var pair = _context.InstrumentPair
                .Include(p => p.BaseInstrument)
                .Include(p => p.QuoteInstrument)
                .Where(i => i.Symbol == pairSymbol).First();

            var history = _context.PriceHistory.Where(h => 
                h.InstrumentPairId == pairSymbol
                && h.Interval == klineInterval
                && startUctTime <= h.UtcOpenTime 
                && h.UtcCloseTime <= endUctTime
            );
            Console.WriteLine("Got current history: " + history.Count());

            if (history.Count() == 0) {
                //fetch from api

                var status = await FetchHistoryFromApi(pairSymbol, _context, klineInterval);
                history = _context.PriceHistory.Where(h =>
                    h.InstrumentPairId == pairSymbol
                    && h.Interval == klineInterval
                    && (startUctTime == -1 || startUctTime <= h.UtcOpenTime)
                    && (endUctTime == -1 || h.UtcCloseTime <= endUctTime)
                );
                Console.WriteLine("Got new history: " + history.Count());
            }

            pair.PriceHistory = history.ToList();//shallow copy



            pair.PriceHistory.Sort((a, b) => a.UtcOpenTime.CompareTo(b.UtcOpenTime));

            return pair;
        }

        [UseProjection]
        public async Task<List<InstrumentPair>> GetInstrumentPairsHistory([Service] TradeListingDbContext _context, string[] pairSymbols, long startUctTime, long endUctTime = -1)
        {
            List<InstrumentPair> result = new List<InstrumentPair>();
            foreach (string pairSymbol in pairSymbols) {
                var instrumentPair = await GetInstrumentPairHistory(_context, pairSymbol, startUctTime, endUctTime);
                result.Add(instrumentPair);
            }
            return result;
        }
    }

}