var builder = DistributedApplication.CreateBuilder(args);

var pg = builder.AddPostgres("ExchangeRates").WithContainerName("exchange-rates-pg").WithLifetime(ContainerLifetime.Persistent).WithPgAdmin();

var project = builder.AddProject<Projects.ExchangeRates>("exchanes-rates").WithReference(pg, "pg").WaitFor(pg);

builder.AddNpmApp("react", "../../frontend/exchange-rates-front")
    .WithReference(project)
    .WaitFor(project)
    .WithEnvironment("BROWSER", "none") // Disable opening browser on npm start
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();