using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	public interface ILogger
	{
		void Debug(string message);
		void Debug(string message, params object[] prms);
		void Error(Exception x);
		void Error(string message);
		void Error(string message, params object[] prms);
		void Fatal(string message);
		void Fatal(Exception x);
		void Fatal(string message, params object[] prms);
		void Info(string message);
		void Info(string message, params object[] prms);
		void Notification(string message);
		void Notification(string message, params object[] prms);
		void Warn(string message);
		void Warn(string message, params object[] prms);
	}
}
