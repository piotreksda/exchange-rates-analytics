namespace ExchangeRates.ExternalDependencies.Nbp;

public static class NbpClientModule
{
    public static IServiceCollection AddNbpClient(this IServiceCollection services, IConfiguration configuration)
    {
        var nbpSection = configuration.GetSection("NbpHttpClient");
        services.Configure<NbpHttpClientOptions>(nbpSection);
        var nbpOptions = new NbpHttpClientOptions();
        nbpSection.Bind(nbpOptions);
        
        services.AddHttpClient<NbpHttpClient>(client =>
        {
            client.BaseAddress = new Uri(nbpOptions.BaseUrl);
        });

        return services;
    }
}