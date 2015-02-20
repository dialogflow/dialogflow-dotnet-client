# api-ai-net

The API.AI .NET Core Library makes it easy to integrate the [API.AI natural language processing API](http://api.ai) into your .NET application. API.AI allows using voice commands and integration with dialog scenarios defined for a particular agent in API.AI.

Library provides simple programming interface for making text and voice requests to the API.AI service. 

## Getting started

### Installation
Library can be installed with Nuget
```
PM> Install-Package ApiAiSDK
```

Or can be downloaded in zip from the [Releases](https://github.com/api-ai/api-ai-net/releases) page.

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
var config = new AIConfiguration("YOUR_SUBSCRIPTION_KEY", "YOUR_CLIENT_ACCESS_TOKEN", SupportedLanguage.English);

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

## Open Source Project Credits

* JSON parsing implemented using modified [fastJSON](https://github.com/xVir/fastJSON) library originally forked from Mehdi Gholam's [repository](https://github.com/mgholam/fastJSON).

