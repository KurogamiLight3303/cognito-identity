namespace CognitoPOC.IdentityApi;

/// <summary>
/// EntryPoint for AWS Lambda
/// </summary>
public class LambdaEntryPoint
    : Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
{
    /// <summary>
    /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
    /// needs to be configured in this method using the UseStartup() method.
    /// </summary>
    /// <param name="builder"></param>
    protected override void Init(IWebHostBuilder builder)
    {
        try
        {
            ILoggingBuilder? loggingBuilder = null; 
            Console.WriteLine(@"LambdaEntryPoint - init");
            builder
                .ConfigureAppConfiguration((context, _) =>
                {
                    Console.WriteLine(@"LambdaEntryPoint - env::" + context.HostingEnvironment.EnvironmentName);
                })
                .ConfigureLogging((logging) =>
                {
                    Console.WriteLine(@"LambdaEntryPoint - logging");
                    loggingBuilder = logging;
                })
                .UseStartup((_) => new LambdaStartUp(loggingBuilder));
        }
        catch (Exception ex)
        {
            Console.WriteLine(@"Exception throw in LambdaEntryPoint: " + ex);
        }
    }

    /// <summary>
    /// Use this override to customize the services registered with the IHostBuilder. 
    /// 
    /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
    /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
    /// </summary>
    /// <param name="builder"></param>
    protected override void Init(IHostBuilder builder)
    {
        builder.UseDefaultServiceProvider(options => options.ValidateScopes = false);
    }
}