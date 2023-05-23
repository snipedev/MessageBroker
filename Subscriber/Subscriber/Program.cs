using System.Net.Http.Json;
using Subscriber.DTOs;

Console.WriteLine("Press ESC to stop");
do
{
    HttpClient client = new HttpClient();
    Console.WriteLine("Listening...");
    while(!Console.KeyAvailable)
    {
        List<int> ackIds = await GetMessageAsync(client);

        Thread.Sleep(2000);

        if(ackIds.Count > 0)
        {
            await AckMessagesAsync(client, ackIds);
        }
        else
        {
            Console.WriteLine("No New Messaes")
        }
    }

} while (Console.ReadKey(true).Key != ConsoleKey.Escape);


static async Task<List<int>> GetMessageAsync(HttpClient httpClient)
{
    List<int> ackids = new();
    List<MessageReadDTO> newMessages = new();
    try
    {
        newMessages = await httpClient.GetFromJsonAsync<List<MessageReadDTO>>("https://localhost:7163/api/subscriptions/2/messages");
    }
    catch (Exception ex)
    {
        return ackids;
    }
    foreach (var message in newMessages!)
    {
        Console.WriteLine($"{message.Id}-{message.TopicMessage}-{message.MessageStatus}");
        ackids.Add(message.Id);
    }
    return ackids;

}

static async Task AckMessagesAsync(HttpClient httpClient, List<int> ackIds)
{
    var response = await httpClient.PostAsJsonAsync("https://localhost:7163/api/subscriptions/2/messages", ackIds);
    var returnMessage = await response.Content.ReadAsStringAsync(); 

    Console.WriteLine(returnMessage);
}