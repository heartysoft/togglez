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
        private static string ConnectionString = ConfigurationManager.AppSettings["zkConnection"];
        private const string Path = "/toggleztest";

        [Test]
        public void should_set_and_get_value_harness_test()
        {
            setData("{foo:true}");
            var val = getData();

            Assert.AreEqual("{foo:true}", val);
        }

        [Test]
        public void should_get_setting_on_start()
        {
            var runner = new ZkRunner(Path, ConnectionString, TimeSpan.FromSeconds(1));
            var client = runner.Start();

            var reset = new AutoResetEvent(false);

            bool set = false;
            client.SubscribeOn<bool>("foo", x =>
            {
                set = x;
                reset.Set();
            });

            setData("{foo:true}");

            reset.WaitOne(TimeSpan.FromSeconds(2));
            Assert.IsTrue(set);

            setData("{foo:false}");

            reset.WaitOne(TimeSpan.FromSeconds(2));
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
                _zk.Create(Path, Encoding.UTF8.GetBytes(value), Ids.OPEN_ACL_UNSAFE, CreateMode.Ephemeral);
            }
            catch (KeeperException.NodeExistsException)
            {
                _zk.SetData(Path, Encoding.UTF8.GetBytes(value), -1);
            }
        }

        private string getData()
        {
            return Encoding.UTF8.GetString(_zk.GetData(Path, false, new Stat()));
        }
    }
}