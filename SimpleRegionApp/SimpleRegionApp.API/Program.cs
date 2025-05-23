using Amazon;
using Amazon.RDS;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using SimpleRegionApp.API.Core;
using SimpleRegionApp.API.Data;

namespace SimpleRegionApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            var services = builder.Services;
            var config = builder.Configuration;
            var dbEndpoint = Environment.GetEnvironmentVariable("DB_ENDPOINT")
                 ?? throw new Exception("DB_ENDPOINT not set");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD")
                 ?? throw new Exception("DB_PASSWORD not set");
            
            services.AddControllers();
            services.AddDbContextPool<SimpleDbContext>(options =>
                options.UseSqlServer(Authentication.GetConnectionString(dbEndpoint, password)));

            services.AddSingleton<IAmazonS3>(sp =>
            {
                return new AmazonS3Client(RegionEndpoint.USEast1);
            });
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
            services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
            services.AddHostedService<SqsPollingService>();

            var app = builder.Build();
            
            app.MapControllers();
            app.Run();
        }
    }
    
}
