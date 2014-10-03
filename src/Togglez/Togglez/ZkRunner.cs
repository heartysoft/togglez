using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Org.Apache.Zookeeper.Data;
using Togglez.Internal;
using ZooKeeperNet;

namespace Togglez
{
    public class ZkRunner : IWatcher, IDisposable
    {
        private readonly string _path;
        private readonly string _zkConnectionString;
        private readonly TimeSpan _sessionTimeout;
        private ZooKeeper _zk;
        private readonly Internal.Togglez _togglez;

        public static ZkRunnerBuilder New()
        {
            return new ZkRunnerBuilder();
        }

        internal ZkRunner(string path, string zkConnectionString, TimeSpan sessionTimeout)
        {
            _path = path;
            _zkConnectionString = zkConnectionString;
            _sessionTimeout = sessionTimeout;

            _togglez = new Internal.Togglez();
        }

        public Internal.Togglez Start()
        {
            Console.WriteLine("[ZkRunner] starting runner." + DateTime.Now.Ticks);
            _zk = new ZooKeeper(_zkConnectionString, _sessionTimeout, this);
            Console.WriteLine("[ZkRunner] runner started.");
            return _togglez;
        }

        public void Process(WatchedEvent @event)
        {
            Console.WriteLine(@event);
            switch (@event.State)
            {
                case KeeperState.SyncConnected:
                    Console.WriteLine("Connected");
                    connected();
                    break;
                case KeeperState.Disconnected:
                    Console.WriteLine("Disconnected");
                    break;
                case KeeperState.Expired:
                    Console.WriteLine("Expired");
                    expired();
                    break;
            }
        }

        private void expired()
        {
            disposeZk();

            _zk = new ZooKeeper(_zkConnectionString, _sessionTimeout, this);
        }

        private void connected()
        {
            var stat = new Stat();
            string json;

            try
            {
                Console.WriteLine("[ZkRunner] fetching data");
                var settings = _zk.GetData(_path, true, stat);
                json = Encoding.UTF8.GetString(settings);
            }
            catch (KeeperException.NoNodeException)
            {
                Console.WriteLine("[ZkRunner] Node not found.");
                if (_zk.Exists(_path, true) != null)
                {
                    Console.WriteLine("[ZkRunner] Node exists...connecting...");
                    connected();
                }

                return;
            }
            catch (KeeperException.SessionExpiredException)
            {
                expired();
                return;
            }

            _togglez.Set(json);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                disposeZk();
            }
        }

        private void disposeZk()
        {
            var cancellationToken = new CancellationTokenSource();
            var task = Task.Factory.StartNew(() => _zk.Dispose(), cancellationToken.Token);
            var timeout = Task.Delay(3000, cancellationToken.Token);

            Task.WhenAny(task, timeout);
            cancellationToken.Cancel();
        }
    }
}