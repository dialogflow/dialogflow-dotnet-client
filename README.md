api.ai: .NET Library
==============

[![Build Status](https://travis-ci.org/api-ai/apiai-dotnet-client.svg?branch=master)](https://travis-ci.org/api-ai/apiai-dotnet-client)
[![Nuget Version](https://img.shields.io/nuget/v/ApiAiSDK.svg)](https://www.nuget.org/packages/ApiAiSDK/)

The api.ai .NET Library makes it easy to integrate the [API.AI natural language processing API](http://api.ai) into your .NET application. API.AI allows using voice commands and integration with dialog scenarios defined for a particular agent in API.AI.

Library provides simple programming interface for making text and voice requests to the API.AI service. 

## Getting started

### Installation
Library can be installed with Nuget
```
PM> Install-Package ApiAiSDK
```

Or can be downloaded as sources from the [Releases](https://github.com/api-ai/api-ai-net/releases) page.

### Usage

Assumed you already have API.AI account and have at least one agent configured. If no, please see [documentation](http://api.ai/docs/index.html) on the API.AI website.

First, add following usages to your module:
```csharp
using ApiAiSDK;
using ApiAiSDK.Model;
```

Then add `ApiAi` field to your class:
```csharp
private ApiAi apiAi;
```

Now you need to initialize `ApiAi` object with appropriate access keys and language.
```csharp
var config = new AIConfiguration("YOUR_CLIENT_ACCESS_TOKEN", SupportedLanguage.English);
apiAi = new ApiAi(config);
```

Done! Now you can easily do requests to the API.AI service 
* using `TextRequest` method for simple text requests
    ```csharp
    var response = apiAi.TextRequest("hello");
    ```

* using `VoiceRequest` method for voice binary data in PCM (16000Hz, Mono, Signed 16 bit) format
    ```csharp
    var response = apiAi.VoiceRequest(voiceStream);
    ```

Also see [unit tests](https://github.com/api-ai/api-ai-net/blob/master/ApiAiSDK.Tests/ApiAiTest.cs) for more examples.

## Windows Phone 8

Windows Phone version has some additional features such as system speech recognition for easy API.AI service integration.
After installing the library you should add permissions for Internet and Sound recording to your app.
Currently, speech recognition is performed using Windows Phone System speech recognition. So, you must be sure languages you are using is installed on device (It can be checked on Settings->speech screen of device).

To use special features you need to use `AIService` class instead of `ApiAi` class. 

### Initialization 

First, you need to initialize AIConfiguration object with your keys and desired language.

```csharp
var config = new AIConfiguration("client access token", SupportedLanguage.English);
```

Second, create AIService object using the configuration object.

```csharp
var aiService = AIService.CreateService(config);
```

Now you need add handlers for OnResult and OnError events

```csharp
aiService.OnResult += aiService_OnResult;
aiService.OnError += aiService_OnError;
```

And at the end call Initialization method

```csharp
await aiService.InitializeAsync();
```

The entire code snippet:

```csharp
try
{
    var config = new AIConfiguration("client access token", SupportedLanguage.English);

    aiService = AIService.CreateService(config);
    aiService.OnResult += aiService_OnResult;
    aiService.OnError += aiService_OnError;
    await aiService.InitializeAsync();
}
catch (Exception e)
{
    // Some exception processing
}
```

### Using API.AI

Now you can use methods for listening and requesting results from server, all you need to call `StartRecognitionAsync` method (don't forget to use `await` operator, otherwise you will not be able to catch some processing exceptions)

```csharp
try
{
    await aiService.StartRecognitionAsync();
}
catch (Exception exception)
{
    // Some exception processing
}
```

### Results processing

Results will be passed to the `OnResult` handler, most of errors will be passed to the `OnError` handler. Don't forget to use dispatcher when working with UI, because of handlers can be called from the Background thread.

```csharp
void aiService_OnError(AIServiceException error)
{
    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
    {
        // sample error processing
        ResultTextBlock.Text = error.Message;
    });
}

void aiService_OnResult(ApiAiSDK.Model.AIResponse response)
{
    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
    {
        // sample result processing
        ResultTextBlock.Text = response.Result.ResolvedQuery;
    });
}
```

## Universal Windows Platform

UWP version of the library is similar to Windows Phone version except some differences in API.
After installing the library you should add capabilities for **Internet(Client)** and **Microphone** to your app.
Currently, speech recognition is performed using `Windows.Media.SpeechRecognition` speech recognition. So, you must be sure languages you are using is installed on device.
API for the platform uses async/await feature. So, you don't need to set up any callbacks.

To use special features you need to use `AIService` class instead of `ApiAi` class. 

### Initialization 

First, you need to initialize AIConfiguration object with your keys and desired language.

```csharp
var config = new AIConfiguration("client access token", SupportedLanguage.English);
```

Second, create AIService object using the configuration object.

```csharp
var aiService = AIService.CreateService(config);
```

And at the end call Initialization method

```csharp
await aiService.InitializeAsync();
```

The entire code snippet:

```csharp
try
{
    var config = new AIConfiguration("client access token", SupportedLanguage.English);
    aiService = AIService.CreateService(config);
    await aiService.InitializeAsync();
}
catch (Exception e)
{
    // Some exception processing
}
```

### Using API.AI

Now you can use methods for listening and requesting results from server, all you need to call `StartRecognitionAsync` method (don't forget to use `await` operator, otherwise you will not be able to catch some processing exceptions)

```csharp
try
{
    var response = await aiService.StartRecognitionAsync();
}
catch (Exception exception)
{
    // Some exception processing
}
```

### Results processing

Results will be in the `response` variable.

## Open Source Project Credits

* JSON parsing implemented using [Json.NET](http://www.newtonsoft.com/json).

