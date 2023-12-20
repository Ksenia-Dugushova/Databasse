using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Microsoft.Data.SqlClient;
using movies;

namespace movies
{
    public class Program
    {

        static void Main(string[] args)
        {
            // using (var context = new MoviesDbContext())
            {
                Stopwatch stopwatch = new Stopwatch();

                /////////////
                string getWord(string line, int wordIndex, char separator, out int wordIndexLocation, int startIndex = 0)
                {
                    wordIndexLocation = 0;
                    int currentWordIndex = 0;

                    while (startIndex < line.Length)
                    {
                        if (line[startIndex] != separator)
                        {
                            int wordStart = startIndex;

                            while (startIndex < line.Length && line[startIndex] != separator)
                            {
                                startIndex++;
                            }

                            currentWordIndex++;
                            if (currentWordIndex == wordIndex)
                            {
                                wordIndexLocation = startIndex + 1;
                                return line.Substring(wordStart, startIndex - wordStart);
                            }
                        }
                        else
                        {
                            currentWordIndex++;
                        }

                        startIndex++;
                    }

                    return $"Меньше {wordIndex} слов в строке";
                }

                Console.WriteLine("Поехали!");
                stopwatch.Start();

                string filePath1 = "C:\\Users\\Пользователь\\Прога_фильмы\\MovieCodes_IMDB.txt";

                ConcurrentDictionary<string, Movie> code_movies = new ConcurrentDictionary<string, Movie>();
                ConcurrentDictionary<string, string> tittle_to_moviescode = new ConcurrentDictionary<string, string>();

                Parallel.ForEach(File.ReadLines(filePath1), line =>
                {
                    string region = getWord(line, 4, '\t', out int ind4);
                    string language = getWord(line, 1, '\t', out int ind5, ind4);

                    if ((region == "RU" || region == "US" || region == "GB") || (language == "ru" || language == "en"))
                    {
                        string movieCode1 = getWord(line, 1, '\t', out int ind1);
                        string movieTitle = getWord(line, 2, '\t', out int ind3, ind1);

                        if (!tittle_to_moviescode.ContainsKey(movieTitle))
                        {
                            tittle_to_moviescode.TryAdd(movieTitle, movieCode1);
                        }

                        code_movies.AddOrUpdate(movieCode1,
                            addValueFactory: key => new Movie(movieTitle, key),
                            updateValueFactory: (key, existingMovie) =>
                            {
                                lock (existingMovie)
                                {
                                    existingMovie.movieTitles = movieTitle;
                                }
                                return existingMovie;
                            });
                    }
                });
                /*
                using (ApplicationContext db = new ApplicationContext(true))
                {
                    int k = 0;
                    foreach (var movie in code_movies)
                    {
                        k++;
                        db.Add(movie.Value);

                        if (k % 10000 == 0)
                        {
                            Console.WriteLine("есть" + k);
                        }

                    }
                    db.SaveChanges();

                }*/

                /*
                foreach (var code in tittle_to_moviescode)
                {
                    Console.WriteLine(code.Key + " " +  code.Value);
                }
                */
                /*
                foreach (var code in code_movies)
                {
                    Console.WriteLine(code.Key + " " + code.Value.movieTitles.First());
                }
                */

                Console.WriteLine("считали первый файл" + stopwatch.Elapsed);

                ///////////////////////////////////Рейтинг по коду фильма

                string filePath2 = "C:\\Users\\Пользователь\\Прога_фильмы\\Ratings_IMDB.txt";

                Parallel.ForEach(File.ReadLines(filePath2), line =>
                {
                    string movieCode2 = getWord(line, 1, '\t', out int ind1);
                    string movieRating = getWord(line, 1, '\t', out int ind2, ind1);

                    if (code_movies.TryGetValue(movieCode2, out Movie movie))
                    {
                        code_movies[movieCode2].movieRating = movieRating;
                    }
                });
                /*
                foreach (var code in code_movies)
                {
                    Console.WriteLine(code.Key + " " + code.Value.movieRating);
                }
                */

                Console.WriteLine("считали второй файл" + stopwatch.Elapsed);


                ////////////////////////////////Тэги и их айди
                string filePath3 = "C:\\Users\\Пользователь\\Прога_фильмы\\TagCodes_MovieLens.txt";

                // ConcurrentDictionary<string, string> tagCodes = new ConcurrentDictionary<string, string>();

                //Parallel.ForEach(File.ReadLines(filePath3), line =>
                //{
                //  string idTag = getWord(line, 1, ',', out int ind1);
                //string tagName = getWord(line, 1, ',', out int ind2, ind1);

                //tagCodes.TryAdd(idTag, tagName);

                //});


                ConcurrentDictionary<string, Tag> tagCodes = new ConcurrentDictionary<string, Tag>();

                Parallel.ForEach(File.ReadLines(filePath3), line =>
                {
                    string idTag = getWord(line, 1, ',', out int ind1);
                    string tagName = getWord(line, 1, ',', out int ind2, ind1);

                    tagCodes.TryAdd(idTag, new Tag(idTag, tagName));
                });

                Console.WriteLine("считали третий файл" + stopwatch.Elapsed);



                ////////////////////////////////////////////////
                string filePath4 = "C:\\Users\\Пользователь\\Прога_фильмы\\links_IMDB_MovieLens.txt";

                ConcurrentDictionary<string, string> idAndCode = new ConcurrentDictionary<string, string>();

                Parallel.ForEach(File.ReadLines(filePath4), line =>
                {
                    string movieId = getWord(line, 1, ',', out int ind1);
                    string imdbId = "tt" + getWord(line, 1, ',', out int ind2, ind1);

                    idAndCode.TryAdd(movieId, imdbId);
                });
                /*
                foreach (var code in idAndCode)
                {
                    Console.WriteLine(code.Key + " " + code.Value);
                }
                */

                Console.WriteLine("считали четвёртый файл" + stopwatch.Elapsed);



                /////////////////////////айди фильма по айди тэга
                string filePath5 = "C:\\Users\\Пользователь\\Прога_фильмы\\TagScores_MovieLens.txt";

                ConcurrentDictionary<string, HashSet<Movie>> tagToMovieId = new ConcurrentDictionary<string, HashSet<Movie>>();

                Parallel.ForEach(File.ReadLines(filePath5), line =>
                {
                    string relevance = getWord(line, 3, ',', out int ind3);

                    if (Convert.ToInt32(relevance[2]) >= Convert.ToInt32('5'))
                    {
                        string movieID = getWord(line, 1, ',', out int ind1);
                        string idTag = getWord(line, 1, ',', out int ind2, ind1);

                        if (idAndCode.TryGetValue(movieID, out string codeMovie))
                        {
                            lock (tagToMovieId)
                            {
                                if (code_movies.TryGetValue(codeMovie, out Movie movie))
                                {
                                    code_movies[codeMovie].tag.Add(tagCodes[idTag]);
                                }
                            }

                            if (idAndCode.TryGetValue(codeMovie, out string imdbId))
                            {
                                // Проверяем, есть ли фильм с этим IMDb идентификатором в нашем словаре
                                if (code_movies.TryGetValue(imdbId, out Movie movie))
                                {
                                    if (tagToMovieId.TryGetValue(idTag, out HashSet<Movie> codes))
                                    {
                                        codes.Add(movie);
                                    }
                                    else
                                    {
                                        tagToMovieId.GetOrAdd(idTag, _ => new HashSet<Movie> { movie });
                                    }
                                }
                            }
                        }
                    }
                });

                /*
                foreach (var tag in tagToMovieId)
                {
                    Console.WriteLine(tag.Key + " " + tag.Value.ToArray<string>().First());
                }
                */
                Console.WriteLine("считали пятый файл" + stopwatch.Elapsed);

                //////////////////////////////////////////
                string filePath6 = "C:\\Users\\Пользователь\\Прога_фильмы\\ActorsDirectorsNames_IMDB.txt";
                ConcurrentDictionary<string, Person> nconstNamePerson = new ConcurrentDictionary<string, Person>();

                Parallel.ForEach(File.ReadLines(filePath6), line =>
                {
                    string nconst = getWord(line, 1, '\t', out int ind1);
                    string primaryName = getWord(line, 1, '\t', out int ind2, ind1);


                    nconstNamePerson[nconst] = new Person(primaryName, nconst);
                });

                try
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        int t = 0;
                        Console.WriteLine("SAFCJHAEEEEEEEEEEEEEEGJDVJKREGBUIEHRGBSHVUHRGUVRSHB;");
                        foreach (var movie in code_movies)
                        {
                            Console.WriteLine("SAFCJHAEEEEEEEEEEEEEEGJDVJKREGBUIEHRGBSHVUHRGUVRSHB;");
                            t++;
                            try
                            {
                                db.Movies.Add(movie.Value);
                                //db.SaveChanges();
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine($"Ошибка при добавлении фильма '{movie.Value.movieTitles}': {innerEx.Message}");
                            }

                            if (t % 1 == 0)
                            {
                                Console.WriteLine("естьььь" + t);
                            }
                        }



                        foreach (var movie in code_movies.Values)
                        {
                            // Добавляем фильм в контекст
                            db.Movies.Add(movie);

                            // Добавляем каждого актера в контекст
                            foreach (var actor in movie.actors)
                            {
                                db.Actors.Add(actor);
                            }
                        }


                        int k = 0;
                        foreach (var actor in nconstNamePerson)
                        {
                            k++;
                            try
                            {
                                db.Actors.Add(actor.Value);
                                //db.SaveChanges();
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine($"Ошибка при добавлении актера '{actor.Value.Name}': {innerEx.Message}");
                            }

                            if (k % 1 == 0)
                            {
                                Console.WriteLine("есть" + k);
                            }
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла общая ошибка при добавлении данных: {ex.Message}");
                }


                /*
                Console.WriteLine("SAFCJHAEEEEEEEEEEEEEEGJDVJKREGBUIEHRGBSHVUHRGUVRSHB;");
                using (ApplicationContext db = new ApplicationContext())
                {
                    int t = 0;
                    Console.WriteLine("SAFCJHAEEEEEEEEEEEEEEGJDVJKREGBUIEHRGBSHVUHRGUVRSHB;");
                    foreach (var movie in code_movies)
                    {
                        Console.WriteLine("SAFCJHAEEEEEEEEEEEEEEGJDVJKREGBUIEHRGBSHVUHRGUVRSHB;");
                        t++;
                        db.Add(movie.Value);

                        if (t % 1 == 0)
                        {
                            Console.WriteLine("естьььь" + t);
                        }

                    }
                    db.SaveChanges();

                    int k = 0;
                    foreach (var actor in nconstNamePerson)
                    {
                        k++;
                        db.Add(actor.Value);

                        if (k % 1 == 0)
                        {
                            Console.WriteLine("есть" + k);
                        }

                    }
                    db.SaveChanges();


                }*/



                /*
                foreach (var code in nconstNamePerson)
                {
                    Console.WriteLine(code.Key + " " + code.Value);
                }
                */
                Console.WriteLine("считали седьмой файл" + stopwatch.Elapsed);

                string filePath7 = "C:\\Users\\Пользователь\\Прога_фильмы\\ActorsDirectorsCodes_IMDB.txt";
                Parallel.ForEach(File.ReadLines(filePath5), line =>
                {
                    string category = getWord(line, 4, '\t', out int ind4);
                    if (category == "director" || category == "actor")
                    {
                        string tconst = getWord(line, 1, '\t', out int ind1);
                        string nconst = getWord(line, 2, '\t', out int ind2, ind1);

                        if (category == "actor")
                        {
                            if (code_movies.TryGetValue(tconst, out Movie movie) && nconstNamePerson.TryGetValue(nconst, out Person actor))
                            {
                                movie.actors.Add(actor);
                                actor.AddMovie(movie);
                            }
                        }

                        if (category == "director")
                        {
                            if (code_movies.TryGetValue(tconst, out Movie movie) && nconstNamePerson.TryGetValue(nconst, out Person director))
                            {
                                if (movie.director == null)
                                {
                                    movie.director = new List<Person>();

                                }
                                movie.director.Add(director);
                                director.AddMovie(movie);
                            }
                            else
                            { return; }
                        }
                    }

                });

                /*
                //tconst и соответствующий nconst
                foreach (var code in actorDirectorCodes)
                {
                    Console.WriteLine(code.Key + " " + code.Value);
                }
                */

                /*
                //nconst и category
                foreach (var code in ncontCategory)
                {
                    Console.WriteLine(code.Key + " " + code.Value);
                }
                */

                //}
                Console.WriteLine("считали шестой файл" + stopwatch.Elapsed);


                Console.Write("Если вы хотите получить информацию о фильме по его названию, нажмите 1; \n " +
                    "Если хотите получить список фильмов, в которых снимался актёр, нажмите 2; \n" +
                    "Если хотите получить список фильмов по тэгу, нажмите 3 \n");

                string userInput = Console.ReadLine();
                if (userInput == "1")
                {
                    Console.WriteLine("Введите название фильма");
                    string userInputFilm = Console.ReadLine();
                    if (tittle_to_moviescode.TryGetValue(userInputFilm, out string movieCode))
                    {
                        if (code_movies.TryGetValue(movieCode, out Movie movie))
                        {
                            Console.WriteLine($"Название фильма: {movie.movieTitles.First()}");
                            Console.WriteLine($"Тэг:");
                            foreach (var tag in movie.tag)
                            {
                                Console.WriteLine($" {tag}");
                            }
                            Console.WriteLine($"Рейтинг: {movie.movieRating}");
                            Console.WriteLine("Список актеров:");
                            foreach (var actor in movie.actors)
                            {
                                Console.WriteLine($"  {actor.Name}");
                            }
                            Console.WriteLine("Режиссер:");
                            foreach (var director in movie.director)
                            {
                                Console.WriteLine($"  {director.Name}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Фильм с названием {userInputFilm} не найден.");
                    }
                }

                if (userInput == "2")
                {
                    Console.WriteLine("Введите имя актёра");
                    string userInputActor = Console.ReadLine();

                    Person actorPerson = null;
                    foreach (var person in nconstNamePerson.Values)
                    {
                        if (person.Name == userInputActor)
                        {
                            actorPerson = person;
                            break;
                        }
                    }

                    if (actorPerson != null)
                    {
                        Console.WriteLine($"Фильмы, в которых снимался актёр {userInputActor}:");
                        foreach (var movie in actorPerson.Movies)
                        {

                            Console.WriteLine($"- {movie.movieTitles.First()}");

                        }
                    }
                    else
                    {
                        Console.WriteLine($"Актёр с именем {userInputActor} не найден.");
                    }
                }
                /*
                if (userInput == "3")
                {
                    Console.WriteLine("Введите тег:");
                    string userInputTag = Console.ReadLine();

                    if (tagToMovieId.TryGetValue(userInputTag, out HashSet<string> movieCodes))
                    {
                        Console.WriteLine($"Фильмы с тегом {userInputTag}:");
                        foreach (var movieCode in movieCodes)
                        {
                            if (code_movies.TryGetValue(movieCode, out Movie movie))
                            {
                                Console.WriteLine($"- {movie.movieTitles.First()}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Фильмы с тегом {userInputTag} не найдены.");
                    }
                }*/
                /*
                //  сохранение в базу данных
                foreach (var movie in code_movies.Values)
                {
                    context.Movies.Add(movie);
                }

                context.SaveChanges();
                */
            }
        }
    }
}