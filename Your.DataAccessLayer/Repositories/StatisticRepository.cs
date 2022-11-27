using JsonServerKit.DataAccess;
using NHibernate;
using Your.Domain.BusinessObjects;

namespace Your.DataAccessLayer.Repositories
{
    public class StatisticRepository : IRepository<Statistic>
    {
        public void Create(Statistic entity)
        {
            using (ISession session = SessionManager.OpenSession())
            //using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(entity);
                //transaction.Commit();
            }
        }

        public Statistic Read(object id)
        {
            throw new NotImplementedException();
        }

        public void Update(Statistic entity)
        {
            throw new NotImplementedException();
        }

        public Statistic CreateOrUpdate(Statistic entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(object id)
        {
            throw new NotImplementedException();
        }

        public void Delete(Statistic entity)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll()
        {
            throw new NotImplementedException();
        }
    }
}
