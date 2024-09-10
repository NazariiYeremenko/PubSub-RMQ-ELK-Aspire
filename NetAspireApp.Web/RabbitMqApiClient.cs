namespace NetAspireApp.Web;

public class RabbitMqApiClient(HttpClient httpClient)
{
    public async Task PostMessageToRabbitMq()
    {
        await httpClient.PostAsync("/post-message-rmq", null);
    }
}
