using Microsoft.EntityFrameworkCore;

namespace RestApi.Models
{
    public class TaskContext : DbContext
    {
        public DbSet<ToDoTask> Tasks { get; set; }
        public TaskContext(DbContextOptions<TaskContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ToDoTask>().Property<bool>("IsDeleted");
            builder.Entity<ToDoTask>().HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
