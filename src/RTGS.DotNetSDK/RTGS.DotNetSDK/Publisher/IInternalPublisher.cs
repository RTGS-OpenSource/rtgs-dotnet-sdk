using System.Runtime.CompilerServices;

namespace RTGS.DotNetSDK.Publisher;

internal interface IInternalPublisher : IAsyncDisposable
{
	Task<SendResult> SendMessageAsync<TMessage>(
		TMessage message,
		CancellationToken cancellationToken,
		Dictionary<string, string> headers = null,
		[CallerMemberName] string callingMethod = null);

	Task<SendResult> SendMessageAsync<TMessage>(
		TMessage message,
		string messageIdentifier,
		CancellationToken cancellationToken,
		Dictionary<string, string> headers = null,
		[CallerMemberName] string callingMethod = null);
}
