using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DynamicLinqEnumAsParam
{
    class Program
    {
        private BloggingContext _context;
        public const string ConnectionString = @"SET CONNECTION STRING";

        static void Main(string[] args)
        {
            var now = DateTime.Now;
            Console.WriteLine("Application started.");

            var program = new Program();
            program.Run().Wait();

            Console.WriteLine($"Application ended in {(DateTime.Now - now)}.");

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private async Task Run()
        {
            this._context = new BloggingContext();
            await this._context.Database.MigrateAsync();
            //await this.GenerateData(100);

            //OK
            var result1 = await _context.Blogs.Where(t => t.Enum == MyEnum.Second).ToArrayAsync();
            //OK
            var result2 = await _context.Blogs.Where("Enum=1").ToDynamicArrayAsync();
            //ERROR !!!!!!
            var result3 = await _context.Blogs.Where("Enum=@0", 1).ToDynamicArrayAsync();
        }

        private async Task GenerateData(int iterations)
        {
            var r = new Random();
            for (var i = 0; i < iterations; i++)
            {
                var list = new List<Post>();
                for (var j = 0; j < 10; j++)
                    list.Add(new Post
                    {
                        Content = Extensions.GenerateRandomString(500),
                        Title = Extensions.GenerateRandomString(20),
                        Entered = DateTime.UtcNow
                    });
                this._context.Blogs.Add(new Blog
                {
                    Rating = r.Next(1, 10),
                    Url = Extensions.GenerateRandomString(100),
                    Enum = r.Next(1, 10) < 3 ? (MyEnum?)null : (MyEnum)r.Next(0, 3),
                    Posts = list
                });
            }

            await this._context.SaveChangesAsync();
        }
    }

    public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Program.ConnectionString);
        }
    }

    public enum MyEnum
    {
        First,
        Second,
        Third
    }

    public class Blog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Url { get; set; }
        public int Rating { get; set; }
        public MyEnum? Enum { get; set; }
        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public DateTime Entered { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }

    public class Extensions
    {
        private static string UpperCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static string LowerCharacters = "abcdefghijklmnoprstuvzxyqw";
        private static string NumberCharacters = "0123456789";
        private static string SpecialCharacters = ",.-_!#$%&/()=";

        /// <summary>
        /// Generates the random string.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="chars">The chars.</param>
        /// <returns>System.String.</returns>
        public static string GenerateRandomString(int length, string chars = null)
        {
            var random = new Random();

            return new string(Enumerable.Repeat(chars ?? UpperCharacters + LowerCharacters + NumberCharacters + SpecialCharacters, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
