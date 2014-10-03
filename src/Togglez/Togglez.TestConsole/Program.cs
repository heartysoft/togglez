using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Org.Apache.Zookeeper.Data;
using ZooKeeperNet;

namespace Togglez.TestConsole
{
    class Program 
    {
        //need to create this setting in zk first. It should have a json payload with {foo:someInteger}.
        private const string _path = "/testharness";

        static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            var zk = ZkRunner.New()
                .Path(() => _path)
                .ConnectionString(() => "192.168.60.2:2181")
                .SessionTimeout(() => TimeSpan.FromSeconds(3))
                .Build();

            var togglez = zk.Start();

            Console.WriteLine(togglez.Get<int>("foo"));
            togglez.SubscribeOn<int>("foo", Console.WriteLine);
            Console.WriteLine(togglez.Get<int>("foo"));


            //waiting synchronously is optional.
            Console.WriteLine("Waiting for settings...");
            togglez.WaitForFirstSettings(TimeSpan.FromSeconds(10));
            Console.WriteLine("Got settings..."); 

            Console.ReadLine();
            zk.Dispose();
        }

    }
}
