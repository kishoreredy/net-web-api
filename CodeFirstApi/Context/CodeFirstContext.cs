using CodeFirstApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstApi.Context
{
    public class CodeFirstContext : DbContext
    {
        public CodeFirstContext(DbContextOptions<CodeFirstContext> options) : base(options) { }
        public DbSet<Employee> Users { get; set; }
        public DbSet<Salary> Salaries { get; set; }
    }
}
