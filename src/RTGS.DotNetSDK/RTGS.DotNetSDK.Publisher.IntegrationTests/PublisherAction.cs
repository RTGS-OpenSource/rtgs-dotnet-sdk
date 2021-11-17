using System;
using System.Threading;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class PublisherAction<TRequest>
	{
		private readonly Func<IRtgsPublisher, TRequest, CancellationToken, Task<SendResult>> _sendDelegate;

		public PublisherAction(TRequest request, string instructionType, Func<IRtgsPublisher, TRequest, CancellationToken, Task<SendResult>> sendDelegate)
		{
			_sendDelegate = sendDelegate;
			InstructionType = instructionType;
			Request = request;
		}

		public string InstructionType { get; }

		public TRequest Request { get; }

		public Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default) =>
			_sendDelegate(publisher, Request, cancellationToken);
	}
}
