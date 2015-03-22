namespace Togglez.Internal
{
	public class NullLogger : TogglezLogger
	{
		public void InfoFormat(string format, params object[] args)
		{
		}

		public void Info(string message)
		{
		}

		public void DebugFormat(string format, params object[] args)
		{
		}

		public void Debug(string message)
		{
		}

		public void WarnFormat(string format, params object[] args)
		{
		}

		public void Warn(string message)
		{
		}

		public void ErrorFormat(string format, params object[] args)
		{
		}

		public void Error(string message)
		{
		}
	}
}