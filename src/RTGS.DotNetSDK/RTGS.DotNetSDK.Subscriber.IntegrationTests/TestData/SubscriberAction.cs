using System;
using System.Collections.Generic;
using System.Linq;
using RTGS.DotNetSDK.Subscriber.Handlers;
using RTGS.DotNetSDK.Subscriber.IntegrationTests.TestHandlers;

namespace RTGS.DotNetSDK.Subscriber.IntegrationTests.TestData
{
	public class SubscriberAction<TMessage> : ISubscriberAction<TMessage>
	{
		public SubscriberAction(IEnumerable<IHandler> allTestHandlers, Func<IEnumerable<IHandler>, IHandler> selector, string messageIdentifier, TMessage message)
		{
			AllTestHandlers = allTestHandlers.ToList();
			Handler = (ITestHandler<TMessage>)(selector(AllTestHandlers));
			MessageIdentifier = messageIdentifier;
			Message = message;
		}

		public IReadOnlyCollection<IHandler> AllTestHandlers { get; }
		public ITestHandler<TMessage> Handler { get; }
		public string MessageIdentifier { get; }
		public TMessage Message { get; }
	}
}
