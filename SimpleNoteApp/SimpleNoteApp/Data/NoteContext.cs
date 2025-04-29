using Microsoft.EntityFrameworkCore;
using SimpleNoteApp.Models;

namespace SimpleNoteApp.Data
{
    public class NotesContext : DbContext
    {
        public NotesContext(DbContextOptions<NotesContext> options)
            : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; }
    }
}
