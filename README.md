# RTGS.global .NET SDK
This repository contains the source code of the [RTGS.global](https://rtgs.global/) .NET SDK.

## Introduction
The SDK allows clients to communicate with the RTGS.global platform. It consists of two packages.

1. Publisher to send messages
2. Subscriber to receive messages

## Supported Platforms
1.x versions of the SDK require `.NET 6.0` or later.

## Installing
[![NuGet version (RTGS.DotNetSDK.Publisher)](https://img.shields.io/nuget/v/RTGS.DotNetSDK.Publisher.svg?style=flat-square&label=RTGS.DotNetSDK.Publisher)](https://www.nuget.org/packages/RTGS.DotNetSDK.Publisher/) [![NuGet version (RTGS.DotNetSDK.Subscriber)](https://img.shields.io/nuget/v/RTGS.DotNetSDK.Subscriber.svg?style=flat-square&label=RTGS.DotNetSDK.Subscriber)](https://www.nuget.org/packages/RTGS.DotNetSDK.Subscriber/)

NuGet Package Manager:

```shell
PM> Install-Package RTGS.DotNetSDK.Publisher
PM> Install-Package RTGS.DotNetSDK.Subscriber
```

.NET Command Line Interface:

```shell
> dotnet add package RTGS.DotNetSDK.Publisher
> dotnet add package RTGS.DotNetSDK.Subscriber
```

## Building from Source
The SDK can be built in any IDE that supports .NET 6 (e.g. Visual Studio 2022 or Rider 2021.3).

## License
Licensed under the [MIT](LICENSE.md) license.