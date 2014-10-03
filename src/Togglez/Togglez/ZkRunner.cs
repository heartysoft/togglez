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
            _zk = new ZooKeeper(_zkConnectionString, _sessionTimeout, this);
            return _togglez;
        }

        public void Process(WatchedEvent @event)
        {
            Console.WriteLine(@event);
            switch (@event.State)
            {
                case KeeperState.SyncConnected:
                    connected();
                    break;
                case KeeperState.Disconnected:
                    break;
                case KeeperState.Expired:
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
            var settings = _zk.GetData(_path, true, stat);
            var json = Encoding.UTF8.GetString(settings);

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