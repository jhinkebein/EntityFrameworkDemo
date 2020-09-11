using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;

namespace EntityFrameworkDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new MovieContext()) //no constring
            {
                var movie = new Movie
                {
                    Name = "Die Hard",
                    Year = 1988
                };
                db.Movies.Add(movie);
                db.SaveChanges();

                var rating = new Rating
                {
                    RatingDate = Convert.ToDateTime("09/02/1989"),
                    RatingValue = 5,
                    Rater = "Big Brain",
                    MovieId = movie
                };
                db.Ratings.Add(rating);

                rating = new Rating //second to confirm 1:M
                {
                    RatingDate = Convert.ToDateTime("09/21/1991"),
                    RatingValue = 5,
                    Rater = "Rolling Stone",
                    MovieId = movie
                };
                db.Ratings.Add(rating);
                db.SaveChanges();
                
                var query1 = from m in db.Movies orderby m.MovieId select m; //LINQ query
                foreach (var item in query1)
                {
                    Console.WriteLine($"{item.MovieId} {item.Name} {item.Year}");
                }
                

                var query2 = from r in db.Ratings orderby r.RatingId select r;
                foreach (var item in query2)
                {
                    Console.WriteLine($"{item.RatingId} {item.RatingDate} {item.RatingValue} {item.Rater}");
                }

                int maxRating = db.Ratings.Max(p => p.RatingValue); //also linq query
                Console.WriteLine($"Max Rating {maxRating}");

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
       

        public class Movie
        {
            public int MovieId { get; set; }
            public string Name { get; set; }
            public int Year { get; set; }
        }

        public class Rating
        {
            public int RatingId { get; set; }
            public string Rater { get; set; }
            public int RatingValue { get; set; }
            public DateTime RatingDate { get; set; }
            public Movie MovieId { get; set; } //FK
        }

        public class MovieContext : DbContext
        {
            public MovieContext() { }
            public MovieContext(string connString) 
            {
                Database.Connection.ConnectionString = connString;
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                //modelBuilder.Entity<Movie>().MapToStoredProcedures();
                //Rename procedures
                modelBuilder.Entity<Movie>().MapToStoredProcedures
                    (p => p.Insert(sp => sp.HasName("sp_InsertMovie").Parameter(pm => pm.Name, "name").Result
                    (rs => rs.MovieId, "Id")).Update(sp => sp.HasName ("sp_UpdateMovie").Parameter(pm => pm.Name, "name"))
                    .Delete(sp => sp.HasName("sp_DeleteMovie").Parameter(pm => pm.MovieId, "Id"))                   
                    );
            }

            public DbSet<Movie> Movies { get; set; }
            public DbSet<Rating> Ratings { get; set; }
        }
    }
}
