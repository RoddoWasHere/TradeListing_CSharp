using HotChocolate;
using HotChocolate.Data;
using System.Collections.Generic;
using System.Linq;
using TradingAPI.Data;

namespace TradingAPI.Schema
{
    public class Query
    {
        //private readonly MyDbContext _context;



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