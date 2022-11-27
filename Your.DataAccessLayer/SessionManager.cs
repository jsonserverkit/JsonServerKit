using NHibernate;
using NHibernate.Cfg;

namespace Your.DataAccessLayer
{
    public class SessionManager
    {
        private static ISessionFactory _sessionFactory;
        private static readonly string cString = "server=.\\one;database=JsonServerKit_YourClient_Demo;Connect Timeout=2;uid=IUSR_Anonym_Demo;pwd=o_kjllmuq$237";

        private static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    var configuration = new Configuration();
                    configuration.Configure();
                    configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, cString);
                    //configuration.AddAssembly(typeof(Account).Assembly);
                    _sessionFactory = configuration.BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}
