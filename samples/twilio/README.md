# api-ai-twilio-net
Sample .NET agent to integrate api.ai with Twilio SMS service

# Usage

See implementation of the agent in the [TwilioApiAiController.cs](https://github.com/api-ai/api-ai-twilio-net/blob/master/ApiAiService/Controllers/TwilioApiAiController.cs) file.

This web-service uses [API.AI .NET SDK](https://www.nuget.org/packages/ApiAiSDK/) for making requests to the API.AI service and [Twilio.Mvc](https://www.nuget.org/packages/Twilio.Mvc/) for response generation.

Query string passed as the `Body` parameter of the HTTP request.

Short version of code:

```csharp
var textQuery = query.Body;
var apiAiResponse = apiAi.TextRequest(textQuery);
var twilioResponse = new TwilioResponse();
twilioResponse.Sms(apiAiResponse.Result.Fulfillment.Speech);
```

After deployment you can check service with `curl` like:

```
curl -i -X POST -d "Body=hello" http://<your_service_name>.cloudapp.net/api/TwilioApiAi/sms
```

Before deployment `AccessToken` and `SubscriptionKey` must be stored in the ApiAiService Azure configuration.

Also should be provided connection string for diagnostcs storage `Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString`.

Also note, that a similar approach can be used in other cases, not only in ASP.NET MVC Controllers.