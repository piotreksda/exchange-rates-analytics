var builder = DistributedApplication.CreateBuilder(args);

var pg = builder.AddPostgres("ExchangeRates").WithContainerName("exchange-rates-pg").WithLifetime(ContainerLifetime.Persistent).WithPgAdmin();

var project = builder.AddProject<Projects.ExchangeRates>("exchanes-rates").WithReference(pg, "pg").WaitFor(pg);

builder.Build().Run();