﻿using RTGS.ISO20022.Messages.Pacs_008_001.V10;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public abstract class BaseSignedPublisherActionData : BaseActionData
{
	public abstract IPublisherAction<FIToFICustomerCreditTransferV10> PayawayCreate { get; }
}