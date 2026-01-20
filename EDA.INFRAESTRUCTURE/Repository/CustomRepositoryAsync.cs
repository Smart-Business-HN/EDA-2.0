
using EDA.APPLICATION.Repository;
using Ardalis.Specification.EntityFrameworkCore;

namespace EDA.INFRAESTRUCTURE.Repository
{
    public class CustomRepositoryAsync<T> : RepositoryBase<T>, IRepositoryAsync<T> where T : class
    {
        private readonly DatabaseContext db;

        public CustomRepositoryAsync(DatabaseContext db) : base(db)
        {
            this.db = db;
        }
    }
}
