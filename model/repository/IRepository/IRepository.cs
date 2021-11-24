using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CSD_3354_Project_DataAccess.Repository.IRepository
{
  public interface IRepository<T> where T:class
    {
        T Find(int id);
        IEnumerable<T> GetAll(
            Expression<Func<T, bool>> filter = null, // equivalent to where clause
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,   // takes IQueryable<T> as parameter or list of products for example and returns an ordered list of product
            string includeProperties = null, //sort of join in sql
            bool isTracking = true      // can set it to false if only reading from database
            );
        T FirstOrDefault(
            Expression<Func<T, bool>> filter = null,
            string includeProperties = null,
            bool isTracking = true
            );

        void Add(T entity);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entity);
        void Save();
    }
}
