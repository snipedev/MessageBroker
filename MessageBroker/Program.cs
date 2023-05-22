using MessageBroker.Data;
using MessageBroker.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source = MessageBroker.db"));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("api/topics",async(AppDbContext context,Topic topic) => {
    await context.Topics.AddAsync(topic);
    await context.SaveChangesAsync();
    return Results.Created($"api/topics/{topic.Id}",topic);
});

app.MapGet("api/topics",async(AppDbContext context) => {
    var topics = await context.Topics.ToListAsync();
    return Results.Ok(topics);
});

app.MapPost("api/topics/{id}/messages",async(AppDbContext context,int id,Message message) => {
    bool topics = await context.Topics.AnyAsync(t => t.Id == id);
    if(!topics)
    {
        return Results.NotFound("Topic not found, can;t push messages");
    }
    var subs = context.Subscriptions.Where(s => s.TopicId == id);
    if(subs.Count() == 0)
        return Results.NotFound("No Subscriptions for this topic");
    foreach(var sub in subs)
    {
        Message msg = new() {
            TopicMessage = message.TopicMessage,
            SubscriptionId = sub.Id,
            ExpiresAfter = message.ExpiresAfter,
            MessageStatus = message.MessageStatus
        };
        await context.Messages.AddAsync(msg);
    }
    await context.SaveChangesAsync();
    return Results.Ok("message has been added");
});



app.Run();