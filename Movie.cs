using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace movies
{
    public class Movie
    {
        [Key]
        public int id { get; set; }

        public string movieTitles { get; set; }


        //public List<Person> People { get; set; }
        [NotMapped]
        public List<Person> director { get; set; }

        //movie.actor.add(person)
        public List<Person> actors { get; set; }
        [NotMapped]
        public HashSet<Tag> tag = new HashSet<Tag>();

        public string movieRating = "";
        public string MovieCode { get; }

        //public string idTag = "";

        public Movie(string title, string code)
        {
            movieTitles = title;
            MovieCode = code;
            director = new List<Person>();
            actors = new List<Person>();
        }

        public Movie()
        {

        }
    }
}