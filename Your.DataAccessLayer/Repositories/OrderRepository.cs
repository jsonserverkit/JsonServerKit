using JsonServerKit.DataAccess;
using Your.Domain.BusinessObjects;

namespace Your.DataAccessLayer.Repositories
{
    public class OrderRepository : IRepository<Order>
    {
        public void Create(Order entity)
        {
            throw new NotImplementedException();
        }

        public Order Read(object id)
        {
            throw new NotImplementedException();
        }

        public void Update(Order entity)
        {
            throw new NotImplementedException();
        }

        public Order CreateOrUpdate(Order entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(object id)
        {
            throw new NotImplementedException();
        }

        public void Delete(Order entity)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll()
        {
            throw new NotImplementedException();
        }
    }
}
