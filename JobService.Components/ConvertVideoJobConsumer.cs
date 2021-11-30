﻿namespace JobService.Components
{
    using System;
    using System.Threading.Tasks;
    using MassTransit.JobService;
    using Microsoft.Extensions.Logging;


    public class ConvertVideoJobConsumer :
        IJobConsumer<ConvertVideo>
    {
        readonly ILogger<ConvertVideoJobConsumer> _logger;

        public ConvertVideoJobConsumer(ILogger<ConvertVideoJobConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Run(JobContext<ConvertVideo> context)
        {
            _logger.LogInformation($"PROCESANDO JOB {context.Job.Path}");
            var rng = new Random();

            var variance = TimeSpan.FromMilliseconds(rng.Next(8399, 28377));

            await Task.Delay(variance);

            await context.Publish<VideoConverted>(context.Job);
        }
    }
}