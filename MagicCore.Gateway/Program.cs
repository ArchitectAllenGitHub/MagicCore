using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOcelot()
    .AddConsul();
    // .AddConfigStoredInConsul();

var app = builder.Build();

app.UseOcelot();

app.Run();