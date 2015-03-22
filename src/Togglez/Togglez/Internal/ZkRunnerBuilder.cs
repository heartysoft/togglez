using System;

namespace Togglez.Internal
{
    public class ZkRunnerBuilder
    {
        private Func<string> _path;
        private Func<string> _connectionString;
        private Func<TimeSpan> _sessionTimeout;
	    private Func<TogglezLogger> _logger = ()=> new NullLogger();
       
        public ZkRunnerBuilder Path(Func<string> path)
        {
            _path = path;
            return this;
        }

        public ZkRunnerBuilder ConnectionString(Func<string> connectionString)
        {
            _connectionString = connectionString;
            return this;
        }

        public ZkRunnerBuilder SessionTimeout(Func<TimeSpan> sessionTimeout)
        {
            _sessionTimeout = sessionTimeout;
            return this;
        }

	    public ZkRunnerBuilder Logger(Func<TogglezLogger> logger)
	    {
		    _logger = logger;
		    return this;
	    }

        public ZkRunner Build()
        {
            check(_path, "Path");
            check(_connectionString, "ConnectionString");
            check(_sessionTimeout, "SessionTimeout");

            return new ZkRunner(_path(), _connectionString(), _sessionTimeout(), _logger());
        }

        private void check(object target, string name)
        {
            if(target == null)
                throw new ArgumentException(string.Format("Parameter {0} has not been set.", name));
        }
    }
}