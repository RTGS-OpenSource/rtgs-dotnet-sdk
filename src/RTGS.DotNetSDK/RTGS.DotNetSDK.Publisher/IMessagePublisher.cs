using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RTGS.DotNetSDK.Publisher
{
	internal interface IMessagePublisher : IAsyncDisposable
	{
		Task<SendResult> SendMessage<TMessage>(
			TMessage message, 
			string messageIdentifier, 
			CancellationToken cancellationToken, 
			Dictionary<string, string> headers = null, 
			[CallerMemberName] string callingMethod = null);
	}
}
