using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace movies
{
    public class Tag
    {
        [Key]
        public int id { get; set; }
        public string Name { get; set; }
        public HashSet<Movie> Movies = new HashSet<Movie>();

        //tag.Movies.add(movie)
        public Tag() { }

        public Tag(string id, string name)
        {

        }
        public void AddMovie(Movie movieName)
        {
            if (!Movies.Any(m => m.MovieCode == movieName.MovieCode))
            {
                Movies.Add(movieName);
            }
            else
            {

            }
        }
    }
}
