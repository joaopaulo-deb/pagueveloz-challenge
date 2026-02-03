
using Microsoft.EntityFrameworkCore;
using PagueVeloz.Application.Accounts;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Application.Publisher;
using PagueVeloz.Application.Transactions;
using PagueVeloz.Application.Transactions.Operations;
using PagueVeloz.Domain.Contracts;
using PagueVeloz.Repository.Context;
using PagueVeloz.Repository.Publisher;
using PagueVeloz.Repository.Repositories;
using RabbitMQ.Client;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
{   
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
    );
});
builder.Services.AddSwaggerGen(_ =>
{
    _.UseInlineDefinitionsForEnums();
});

builder.Services.AddSingleton<IConnection>(sp =>
{
    //docker
    /*var factory = new ConnectionFactory
    {
        HostName = "pv-rabbitmq",
        UserName = "admin",
        Password = "admin123"
    };*/

    //local
    var factory = new ConnectionFactory
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "admin",
        Password = "admin123"
    };

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddScoped<RabbitMqEventPublisher>();

builder.Services.AddScoped<IEventPublisher>(sp =>
{
    var rabbit = sp.GetRequiredService<RabbitMqEventPublisher>();
    var eventRepository = sp.GetRequiredService<IEventRepository>();

    return new RetryEventPublisher(rabbit, eventRepository, maxAttempts: 5, baseDelayMs: 200);
});


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IOperation, CreditOperation>();
builder.Services.AddScoped<IOperation, DebitOperation>();
builder.Services.AddScoped<IOperation, CaptureOperation>();
builder.Services.AddScoped<IOperation, ReserveOperation>();
builder.Services.AddScoped<IOperation, ReversalOperation>();
builder.Services.AddScoped<IOperation, TransferOperation>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        _ => _.MigrationsAssembly("PagueVeloz.Repository"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
