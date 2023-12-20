using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Formats.Tar;
using Azure;

namespace movies
{
    internal class ApplicationContext : DbContext
    {
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Person> Actors => Set<Person>();
        public DbSet<Tag> Tags => Set<Tag>();


        public ApplicationContext()
        {
            Database.EnsureCreated();
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=trying.db");
            optionsBuilder.EnableSensitiveDataLogging();
            //optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=helloappdb;Trusted_Connection=True;");
        }
        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //  modelBuilder.Entity<Movie>()
        //        .HasMany(m => m.actors)
        //      .WithMany(a => a.Movies);
        // modelBuilder.Entity<Movie>()
        //       .HasMany(m => m.tag)
        //     .WithMany(a => a.Movies);
        // }
    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
