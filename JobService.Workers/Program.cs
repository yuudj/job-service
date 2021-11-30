using JobService.Components;
using MassTransit;
using MassTransit.Definition;
using MassTransit.EntityFrameworkCoreIntegration.JobService;
using MassTransit.JobService.Components.StateMachines;
using MassTransit.JobService.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JobService.Workers
{
    public class Program
    {
        static bool? _isRunningInContainer;
        public static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inDocker) && inDocker;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
             .UseSerilog((ctx, logConfig) =>
             {
                 logConfig.ReadFrom.Configuration(ctx.Configuration);
             })
            .ConfigureServices((hostContext, services) =>
            {

                services.AddDbContext<JobServiceSagaDbContext>(builder =>
               builder.UseSqlServer(hostContext.Configuration.GetConnectionString("JobServiceSql")));

                services.AddMassTransit(x =>
                {
                    x.AddDelayedMessageScheduler();

                    x.AddConsumer<ConvertVideoJobConsumer>(typeof(ConvertVideoJobConsumerDefinition));

                    

                    x.AddSagaRepository<JobSaga>()
                        .EntityFrameworkRepository(r =>
                        {
                            r.ExistingDbContext<JobServiceSagaDbContext>();
                        //r.LockStatementProvider = new PostgresLockStatementProvider();
                    });
                    x.AddSagaRepository<JobTypeSaga>()
                        .EntityFrameworkRepository(r =>
                        {
                            r.ExistingDbContext<JobServiceSagaDbContext>();
                        //r.LockStatementProvider = new PostgresLockStatementProvider();
                    });
                    x.AddSagaRepository<JobAttemptSaga>()
                        .EntityFrameworkRepository(r =>
                        {
                            r.ExistingDbContext<JobServiceSagaDbContext>();
                        //r.LockStatementProvider = new PostgresLockStatementProvider();
                    });

                    x.AddRequestClient<ConvertVideo>();

                    x.SetKebabCaseEndpointNameFormatter();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        if (IsRunningInContainer)
                            cfg.Host("rabbitmq");

                        cfg.UseDelayedMessageScheduler();

                        var options = new ServiceInstanceOptions()
                            .SetEndpointNameFormatter(context.GetService<IEndpointNameFormatter>() ?? KebabCaseEndpointNameFormatter.Instance);

                        cfg.ServiceInstance(options, instance =>
                        {
                            instance.ConfigureJobServiceEndpoints(js =>
                            {
                                js.SagaPartitionCount = 1;
                                js.FinalizeCompleted = true;

                                js.ConfigureSagaRepositories(context);
                            });

                            instance.ConfigureEndpoints(context);
                        });
                    });
                });
                services.AddMassTransitHostedService();

            });
    }
}
