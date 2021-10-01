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

namespace TradingAPI.Schema
{
    public class Query
    {
        //private readonly MyDbContext _context;

        private readonly BinanceClient _client;

        public Query()
        {
            _client = new BinanceClient(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(
                    "WApQfvJsMkriy3TmWqckeJo1z50pxpCGWj6dC1gQ1PhtBLwXkkxIClotV1T1q2W3", //key
                    "rNZbarFqYkTve6RSiWgcsZNPHqg09f8jcd7zA7q6D88VQMJahgXJheVxtaYWJMHT" //secret
                )//TODO: move to config
                // Specify options for the client
            });
        }


        async Task<object> FetchHistoryFromApi(string symbol, MyDbContext _context, KlineInterval klineInterval = KlineInterval.OneDay)
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

        public Book GetBook() =>
            new Book
            {
                Title = "C# in depth.",
                Author = new Author
                {
                    Name = "Jon Skeet"
                }
            };

        [UseProjection]
        public IQueryable<Instrument> GetInstruments([Service] MyDbContext _context) {
            return _context.Instrument;
        }


        //[UseDbContext(typeof(MyDbContext))]
        //[UseProjection]
        //public IQueryable<Instrument> GetInstruments2(
        //     [ScopedService] MyDbContext _context)
        //     => _context.Instrument;

        [UseProjection]
        public IQueryable<InstrumentPair> GetInstrumentPairs([Service] MyDbContext _context){
            //List<InstrumentPair> result = _context.InstrumentPair.ToList();//.AsQueryable<List<InstrumentPair>>();
            return _context.InstrumentPair;
        }

        [UseProjection]
        public IQueryable<InstrumentPair> GetInstrumentPair([Service] MyDbContext _context, string pairSymbol)
        {
            //List<InstrumentPair> result = _context.InstrumentPair.ToList();//.AsQueryable<List<InstrumentPair>>();
            return _context.InstrumentPair.Where(i => i.Symbol == pairSymbol);
        }

        [UseProjection]
        public async Task<InstrumentPair> GetInstrumentPairHistory(
            [Service] MyDbContext _context, 
            string pairSymbol, 
            long startUctTime, 
            long endUctTime = -1,
            KlineInterval klineInterval = KlineInterval.OneDay
        ){

            Console.WriteLine("---Getting InstrumentPairHistory for "+pairSymbol);

            //IQueryable<InstrumentPair>
            var pair = _context.InstrumentPair
                .Include(p => p.BaseInstrument)
                .Include(p => p.QuoteInstrument)
                .Where(i => i.Symbol == pairSymbol).First();

            //var query2 = _context.InstrumentPair
            //    .Join(
            //        _context.PriceHistory,
            //        p => p.Symbol,
            //        h => h.InstrumentPairId,
            //        (pair, history) => pair
            //        //new
            //        //{
            //        //    InvoiceID = invoice.Id,
            //        //    CustomerName = customer.FirstName + "" + customer.LastName,
            //        //    InvoiceDate = invoice.Date
            //        //}
            //    );//.Dis;//.GroupBy(p => p.Symbol).Select(g => g.OrderBy(p => p.Symbol).FirstOrDefault());                
            //      //.Select(g => g.OrderBy(p => p.Symbol).FirstOrDefault());
            //var query = _context.InstrumentPair.Include((i => i.PriceHistory.Where(h => startUctTime <= h.UCTTime && h.UCTTime <= endUctTime));

            //var query = from instPair in _context.Set<InstrumentPair>()
            //            join hist in _context.Set<PriceHistory>()
            //                on instPair.Symbol equals hist.InstrumentPairId
            //            select instPair;// new { instPair, hist };

            //var qTest = query.ToList();


            var history = _context.PriceHistory.Where(h => 
                h.InstrumentPairId == pairSymbol
                && h.Interval == klineInterval
                && startUctTime <= h.UtcOpenTime 
                && h.UtcCloseTime <= endUctTime
            );
            Console.WriteLine("Got current history: " + history.Count());

            if (history.Count() == 0) {
                //fetch from api
                //Action<object> endTimeCondition = (h) => h.UtcCloseTime <= endUctTime;
                //if

                var status = await FetchHistoryFromApi(pairSymbol, _context, klineInterval);
                history = _context.PriceHistory.Where(h =>
                    h.InstrumentPairId == pairSymbol
                    && h.Interval == klineInterval
                    && (startUctTime == -1 || startUctTime <= h.UtcOpenTime)
                    && (endUctTime == -1 || h.UtcCloseTime <= endUctTime)
                );
                Console.WriteLine("Got new history: " + history.Count());
            }
            //var pairCopy = new InstrumentPair { 
            //    Symbol = pair.Symbol,
            //    BaseInstrument = pair.BaseInstrument,
            //    QuoteInstrument = pair.QuoteInstrument,
            //    PriceHistory = history.ToList(),//shallow copy
            //};

            pair.PriceHistory = history.ToList();//shallow copy



            pair.PriceHistory.Sort((a, b) => a.UtcOpenTime.CompareTo(b.UtcOpenTime));
            //var wtf = query.Select(p => p).Distinct();
            return pair;
            //return query.SelectMany().Distinct();
        }

        [UseProjection]
        public async Task<List<InstrumentPair>> GetInstrumentPairsHistory([Service] MyDbContext _context, string[] pairSymbols, long startUctTime, long endUctTime = -1)
        {
            List<InstrumentPair> result = new List<InstrumentPair>();
            foreach (string pairSymbol in pairSymbols) {
                var instrumentPair = await GetInstrumentPairHistory(_context, pairSymbol, startUctTime, endUctTime);
                result.Add(instrumentPair);
            }

            //pair.PriceHistory = history.ToList();//shallow copy
            //var pair = _context.InstrumentPair
            //    .Include(p => p.BaseInstrument)
            //    .Include(p => p.QuoteInstrument)
            //    .Where(i => i.Symbol == pairSymbol).First();
            return result;
        }

        //[UseDbContext(typeof(MyDbContext))]
        //[UseProjection]
        //public IQueryable<InstrumentPair> GetInstrumentPairs2(
        //     [ScopedService] MyDbContext _context)
        //     => _context.InstrumentPair;


        //[UseFirstOrDefault]
        [UseProjection]
        public IQueryable<Student> GetStudentById([Service] MyDbContext context, int studentId) =>
            context.Students.Where(t => t.Id == studentId);

        [UseProjection]
        //[UseFiltering]
        //[UseSorting]
        public IQueryable<Student> GetStudents([Service] MyDbContext context) =>
            context.Students;

        //[UsePaging]
        [UseProjection]
        //[UseFiltering]
        //[UseSorting]
        public IQueryable<Course> GetCourses([Service] MyDbContext context) =>
            context.Courses;

    }

    public class InstrumentPairQuery : InstrumentPair  {

        //public IQuerable<InstrumentQuery GetBaseInstrument(MyDbContext context)
        //{
        //    return context.Instrument;
        //}

    }
    public class InstrumentQuery : InstrumentPair
    {

        //public override Instrument BaseInstrument { 
        //    get {

        //    }; 
        //    set; 
        //}//FK
        //public override Instrument QuoteInstrument { get; set; }//FK

    }


    //public class IntrumentSchema { 

    //}

    //public class InstrumentPair { 

    //}



    public class Book
    {
        public string Title { get; set; }

        public Author Author { get; set; }
    }
    public class Author
    {
        public string Name { get; set; }
    }
}