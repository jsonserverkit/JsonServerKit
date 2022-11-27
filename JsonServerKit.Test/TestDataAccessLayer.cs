using NHibernate;
using NHibernate.Cfg;
using Your.Domain.BusinessObjects;
using Your.DataAccessLayer.Repositories;
using JsonServerKit.DataAccess;
using System.Diagnostics;

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
            var account = new Account { };
            IRepository<Account> repository = new AccountRepository();
            repository.Create(account);
        }

        [TestMethod]
        public void AddStatistic()
        {
            var statisticItem = new Statistic { MessageId = 999, TimeInMsMessageSent = 12345.67d, TimeInMsMessageReceived = 5859d, MessageType = new Account().GetType().ToString() };
            IRepository<Statistic> repository = new StatisticRepository();
            repository.Create(statisticItem);
        }

        [TestMethod]
        public void StopWatchInfo()
        {
            // Display the timer frequency and resolution.
            if (Stopwatch.IsHighResolution)
            {
                Console.WriteLine("Operations timed using the system's high-resolution performance counter.");
            }
            else
            {
                Console.WriteLine("Operations timed using the DateTime class.");
            }

            long frequency = Stopwatch.Frequency;
            Console.WriteLine("  Timer frequency in ticks per second = {0}", frequency);
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
            Console.WriteLine("  Timer is accurate within {0} nanoseconds", nanosecPerTick);
        }

        [TestMethod]
        public void StopWatchTimeSpan()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Thread.Sleep(1000);
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds);
            Console.WriteLine("RunTime " + elapsedTime);
            Console.WriteLine($"Total milliseconds: {ts.TotalMilliseconds}");
            Console.WriteLine($"Total milliseconds with ToString() as N0: {ts.TotalMilliseconds:N0}");
        }
    }
}
