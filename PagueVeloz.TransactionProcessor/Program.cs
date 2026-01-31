
using Microsoft.EntityFrameworkCore;
using PagueVeloz.Application.Accounts;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Application.Customers;
using PagueVeloz.Application.Transactions;
using PagueVeloz.Application.Transactions.Operations;
using PagueVeloz.Repository.Context;
using PagueVeloz.Repository.Repositories;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IOperation, CreditOperation>();
builder.Services.AddScoped<IOperation, DebitOperation>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        _ => _.MigrationsAssembly("PagueVeloz.Repository"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
