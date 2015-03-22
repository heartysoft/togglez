using System;

namespace Togglez
{
	public class ConsoleLogger : TogglezLogger
	{
		public void InfoFormat(string format, params object[] args)
		{
			Console.WriteLine(format, args);
		}

		public void Info(string message)
		{
			Console.WriteLine(message);
		}

		public void DebugFormat(string format, params object[] args)
		{
			Console.WriteLine(format, args);
		}

		public void Debug(string message)
		{
			Console.WriteLine(message);
		}

		public void WarnFormat(string format, params object[] args)
		{
			Console.WriteLine(format, args);
		}

		public void Warn(string message)
		{
			Console.WriteLine(message);
		}

		public void ErrorFormat(string format, params object[] args)
		{
			Console.WriteLine(format, args);
		}

		public void Error(string message)
		{
			Console.WriteLine(message);
		}
	}
}