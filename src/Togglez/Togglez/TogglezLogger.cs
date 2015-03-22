namespace Togglez
{
	public interface TogglezLogger
	{
		void InfoFormat(string format, params object[] args);
		void Info(string message);
		void DebugFormat(string format, params object[] args);
		void Debug(string message);
		void WarnFormat(string format, params object[] args);
		void Warn(string message);
		void ErrorFormat(string format, params object[] args);
		void Error(string message);
	}
}