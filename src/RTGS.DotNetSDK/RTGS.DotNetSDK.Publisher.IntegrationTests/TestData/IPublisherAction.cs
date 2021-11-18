﻿using System.Threading;
using System.Threading.Tasks;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests.TestData
{
	public interface IPublisherAction<TRequest>
	{
		TRequest Request { get; }

		Task<SendResult> InvokeSendDelegateAsync(IRtgsPublisher publisher, CancellationToken cancellationToken = default);
	}
}
