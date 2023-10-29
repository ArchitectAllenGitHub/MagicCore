using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOcelot()
    .AddConsul();
    // .AddConfigStoredInConsul(); 同样的配置,存储在Consul 访问网关404 https://github.com/ThreeMammals/Ocelot/issues/1761

var app = builder.Build();

app.UseOcelot();

app.Run();