using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
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

        private static readonly ILog Logger = LogManager.GetLogger(typeof (ZkRunner));

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
            Logger.InfoFormat("[ZkRunner] starting runner at {0}. Connection: {1}, Path: {2}, Timeout: {3}.", DateTime.Now.Ticks, _zkConnectionString, _path, _sessionTimeout);
            _zk = new ZooKeeper(_zkConnectionString, _sessionTimeout, this);
            return _togglez;
        }

        public void Process(WatchedEvent @event)
        {
            switch (@event.State)
            {
                case KeeperState.SyncConnected:
                    Logger.Info("[ZkRunner] Connected. Fetching data.");
                    connected();
                    break;
                case KeeperState.Disconnected:
                    Logger.Info("[ZkRunner] Disconnected..safe to ignore. Will reconnect automatically.");
                    break;
                case KeeperState.Expired:
                    Logger.Info("[ZkRunner] Expired. Creating another session.");
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
                var settings = _zk.GetData(_path, true, stat);
                json = Encoding.UTF8.GetString(settings);
            }
            catch (KeeperException.NoNodeException)
            {
                Logger.WarnFormat("[ZkRunner] Node {0} not found. Placing watch for node creation.", _path);
                if (_zk.Exists(_path, true) != null)
                {
                    Logger.InfoFormat("[ZkRunner] Node {0} exists...must have been created in the small window between Get and Exists. Fetching data.", _path);
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
                Logger.Debug("Disposing.");
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