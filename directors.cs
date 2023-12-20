using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace movies
{
    public class Person
    {
        [Key]
        public int id { get; set; }

        public string Name { get; set; }
        public List<Movie> Movies { get; set; }

        //actor.Movies.add(movie)

        //public HashSet<string> CodesOfMovies { get; set; }
        [NotMapped]
        public string PerCode { get; set; }
        public Person(string name, string perCode)
        {
            Name = name;
            Movies = new List<Movie>();
            PerCode = perCode;
        }
        public void AddMovie(Movie movie) { Movies.Add(movie); }
        public void Print(Dictionary<string, Movie> MovieByCode)
        {
            Console.WriteLine(Name);
            Console.Write("Участвовал в фильмах: ");
            int q = 0;
            foreach (var movie in Movies)
            {
                if (q > 0) Console.Write(" | ");
                Console.Write(movie.movieTitles.First());
                q++;
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        public Person()
        {

        }
    }
}