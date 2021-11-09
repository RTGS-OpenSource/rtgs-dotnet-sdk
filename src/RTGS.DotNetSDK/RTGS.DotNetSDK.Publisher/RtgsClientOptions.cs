using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTGSDotNetSDK.Publisher
{
	public record RtgsClientOptions
	{
		private RtgsClientOptions(Builder builder)
		{
			BankDid = builder.BankDidValue;
			RemoteHost = builder.RemoteHostValue;
		}

		public string BankDid { get; }
		public string RemoteHost { get; }

		public sealed class Builder
		{
			internal string BankDidValue { get; private set; }
			internal string RemoteHostValue { get; private set; }

			public static Builder CreateNew() => new();

			public Builder BankDid(string bankDid)
			{
				BankDidValue = bankDid;
				return this;
			}

			public Builder RemoteHost(string remoteHost)
			{
				RemoteHostValue = remoteHost;
				return this;
			}

			public RtgsClientOptions Build()
			{
				return new RtgsClientOptions(this);
			}
		}
	}
}
