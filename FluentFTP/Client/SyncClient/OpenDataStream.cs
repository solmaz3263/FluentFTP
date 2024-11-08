﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using FluentFTP.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace FluentFTP {
	public partial class FtpClient {

		/// <summary>
		/// Opens a data stream.
		/// </summary>
		/// <param name='command'>The command to execute that requires a data stream</param>
		/// <param name="restart">Restart location in bytes for file transfer</param>
		/// <returns>The data stream.</returns>
		protected FtpDataStream OpenDataStream(string command, long restart) {
			var type = Config.DataConnectionType;
			FtpDataStream stream = null;

			LogFunction(nameof(OpenDataStream), new object[] { command, restart });

			// The PORT and PASV commands do not work with IPv6 so
			// if either one of those types are set change them
			// to EPSV or EPRT appropriately.
			if (m_stream.LocalEndPoint.AddressFamily == AddressFamily.InterNetworkV6) {
				switch (type) {
					case FtpDataConnectionType.PORT:
						type = FtpDataConnectionType.EPRT;
						Log(FtpTraceLevel.Info, "Changed data connection type to EPRT because we are connected with IPv6.");
						break;

					case FtpDataConnectionType.PASV:
					case FtpDataConnectionType.PASVEX:
					case FtpDataConnectionType.PASVUSE:
						type = FtpDataConnectionType.EPSV;
						Log(FtpTraceLevel.Info, "Changed data connection type to EPSV because we are connected with IPv6.");
						break;
				}
			}

			switch (type) {
				case FtpDataConnectionType.AutoPassive:
				case FtpDataConnectionType.EPSV:
				case FtpDataConnectionType.PASV:
				case FtpDataConnectionType.PASVEX:
				case FtpDataConnectionType.PASVUSE:
					stream = OpenPassiveDataStream(type, command, restart);
					break;

				case FtpDataConnectionType.AutoActive:
				case FtpDataConnectionType.EPRT:
				case FtpDataConnectionType.PORT:
					stream = OpenActiveDataStream(type, command, restart);
					break;
			}

			if (stream == null) {
				throw new InvalidOperationException("The specified data channel type is not implemented.");
			}

			return stream;
		}

	}
}