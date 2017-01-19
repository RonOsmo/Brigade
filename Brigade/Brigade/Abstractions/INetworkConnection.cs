using System;
using System.Collections.Generic;
using System.Text;

namespace Brigade.Abstractions
{
	public interface INetworkConnection
	{
		bool IsConnected { get; }
		void CheckNetworkConnection();
	}
}
