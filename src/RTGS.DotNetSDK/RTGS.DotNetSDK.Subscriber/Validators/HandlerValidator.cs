using System;
using System.Collections.Generic;
using System.Linq;
using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.Subscriber.Validators
{
	public class HandlerValidator : IHandlerValidator
	{
		private readonly IEnumerable<Type> _requiredHandlers = new[]
		{
			typeof(IAtomicLockResponseV1Handler),
			typeof(IAtomicTransferFundsV1Handler),
			typeof(IAtomicTransferResponseV1Handler),
			typeof(IEarmarkCompleteV1Handler),
			typeof(IEarmarkFundsV1Handler),
			typeof(IEarmarkReleaseV1Handler),
			typeof(IMessageRejectV1Handler),
			typeof(IPayawayFundsV1Handler),
			typeof(IPayawayCompleteV1Handler)
		};
		public void Validate(IList<IHandler> handlers)
		{
			var errors = new List<string>();

			if (handlers.Any(handler => handler is null))
			{
				errors.Add("Handlers collection cannot contain null handlers.");
			}

			foreach (Type requiredHandler in _requiredHandlers)
			{
				var configuredHandlers = handlers.Where(requiredHandler.IsInstanceOfType).ToList();
				if (!configuredHandlers.Any())
				{
					errors.Add($"No {requiredHandler.Name} handler was found.");
				}
				else if (configuredHandlers.Count > 1)
				{
					errors.Add($"Multiple handlers of type {requiredHandler.Name} were found.");
				}
			}

			if (errors.Any())
			{
				throw new ArgumentException(string.Join("\r\n", errors), nameof(handlers));
			}
		}
	}
}
