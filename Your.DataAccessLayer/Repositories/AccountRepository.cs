using JsonServerKit.DataAccess;
using NHibernate;
using Your.Domain.BusinessObjects;

namespace Your.DataAccessLayer.Repositories
{
    public class AccountRepository : IRepository<Account>
    {
        public void Create(Account entity)
        {
            using (ISession session = SessionManager.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(entity);
                transaction.Commit();
            }
        }

        public Account Read(object id)
        {
            throw new NotImplementedException();
        }

        public void Update(Account entity)
        {
            throw new NotImplementedException();
        }

        public Account CreateOrUpdate(Account entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(object id)
        {
            throw new NotImplementedException();
        }

        public void Delete(Account entity)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll()
        {
            throw new NotImplementedException();
        }
    }
}
