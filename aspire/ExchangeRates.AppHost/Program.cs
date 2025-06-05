var builder = DistributedApplication.CreateBuilder(args);

var pg = builder.AddPostgres("ExchangeRates").WithContainerName("exchange-rates-pg").WithLifetime(ContainerLifetime.Persistent).WithPgAdmin();

var project = builder.AddProject<Projects.ExchangeRates>("exchanes-rates").WithReference(pg, "pg").WaitFor(pg);

builder.AddNpmApp("react", "../../frontend/exchange-rates-front")
    .WithReference(project)
    .WaitFor(project)
    .WithEnvironment("BROWSER", "none")
    .WithEnvironment("VITE_API_BASE_URL", project.GetEndpoint("http"))
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

// builder.AddDockerComposePublisher();

builder.Build().Run();