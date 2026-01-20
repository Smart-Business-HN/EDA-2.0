using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EDA.INFRAESTRUCTURE
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=eda_db;User Id=root;Password=SomeThingComplicated1234;TrustServerCertificate=True;");

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
