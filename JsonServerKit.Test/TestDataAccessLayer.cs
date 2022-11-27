using NHibernate;
using NHibernate.Cfg;
using Your.Domain.BusinessObjects;
using Your.DataAccessLayer.Repositories;
using JsonServerKit.DataAccess;

namespace JsonServerKit.Test
{
    [TestClass]
    public class TestDataAccessLayer
    {
        private readonly string cString = "server=.\\one;database=JsonServerKit_YourClient_Demo;Connect Timeout=2;uid=IUSR_Anonym_Demo;pwd=o_kjllmuq$237";
        private ISessionFactory _sessionFactory;
        private Configuration _configuration;

        [TestMethod]
        public void AccessDatabase()
        {
            _configuration = new Configuration();
            _configuration.Configure();
            _configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, cString);
            //_configuration.AddAssembly(typeof(Product).Assembly);
            _sessionFactory = _configuration.BuildSessionFactory();
        }


        [TestMethod]
        public void AddAccount()
        {
            var product = new Account { };
            IRepository<Account> repository = new AccountRepository();
            repository.Create(product);
        }
    }
}
