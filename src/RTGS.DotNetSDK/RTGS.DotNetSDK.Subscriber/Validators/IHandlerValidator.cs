using System.Collections.Generic;
using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.Subscriber.Validators
{
	public interface IHandlerValidator
	{
		/// <summary>
		/// Validates the handlers:
		///  - No null handlers
		///	 - No missing handlers (must have a handler for each message type)
		///  - No duplicate handlers (each message should only be handled by one handler)
		/// </summary>
		/// <param name="handlers">The handlers to validate</param>
		void Validate(IReadOnlyList<IHandler> handlers);
	}
}
