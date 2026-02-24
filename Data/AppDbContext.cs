using Microsoft.EntityFrameworkCore;
using NoteList.Models;

namespace NoteList.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Note> Notes => Set<Note>();
    }
}