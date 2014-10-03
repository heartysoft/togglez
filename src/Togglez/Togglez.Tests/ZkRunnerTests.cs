using System;
using System.Configuration;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Org.Apache.Zookeeper.Data;
using ZooKeeperNet;

namespace Togglez.Tests
{
    [TestFixture, Explicit]
    public class ZkRunnerTests
    {
        private ZooKeeper _zk;
        private static readonly string ConnectionString = ConfigurationManager.AppSettings["zkConnection"];
        private const string Path = "/toggleztest";

        [Test]
        public void should_set_and_get_value_harness_test()
        {
            setData("{foo:true}");
            var val = getData();

            Assert.AreEqual("{foo:true}", val);
        }

        [Test]
        public void should_get_setting_on_start_even_if_node_is_added_later()
        {
            var runner =
                ZkRunner.New().Path(() => Path)
                .ConnectionString(() => ConnectionString)
                .SessionTimeout(() => TimeSpan.FromSeconds(5))
                .Build();

            var client = runner.Start();

            var reset = new AutoResetEvent(false);

            bool set = false;
            client.SubscribeOn<bool>("foo", x =>
            {
                Console.WriteLine("[test] got update for foo");
                set = x;
                reset.Set();
            });

            setData("{foo:true}");
            reset.WaitOne(TimeSpan.FromSeconds(10));
            
            Assert.IsTrue(set);

            setData("{foo:false}");
            reset.WaitOne(TimeSpan.FromSeconds(10));

            Assert.IsFalse(set);

            runner.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            _zk = new ZooKeeper(ConnectionString, TimeSpan.FromSeconds(2), null);
        }

        [TearDown]
        public void Teardown()
        {
            _zk.Dispose();
            _zk = null;
        }

        private void setData(string value)
        {
            try
            {
                Console.WriteLine("[test] trying to create");
                _zk.Create(Path, Encoding.UTF8.GetBytes(value), Ids.OPEN_ACL_UNSAFE, CreateMode.Ephemeral);
                Console.WriteLine("Created");
            }
            catch (Exception)
            {
                Console.WriteLine("[test] already exists...setting data.");
                _zk.SetData(Path, Encoding.UTF8.GetBytes(value), -1);
                Console.WriteLine("data set");
            }
        }

        private string getData()
        {
            return Encoding.UTF8.GetString(_zk.GetData(Path, false, new Stat()));
        }
    }
}