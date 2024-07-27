using CodeFirstApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstApi.Context
{
    public class CodeFirstContext(DbContextOptions<CodeFirstContext> options) : DbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Salary> Salaries { get; set; }
    }
}
