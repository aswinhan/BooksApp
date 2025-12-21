using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Postgres;
using Aspire.Hosting.Redis;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// --- Add Database Resource ---
var postgresdb = builder.AddPostgres("postgres")
                        .WithDataVolume("booksapp_postgres_data", isReadOnly: false)
                        .AddDatabase("booksappdb");

// --- Add Redis Resource ---
var redis = builder.AddRedis("redis");


// --- Add Backend API Project ---
var backendApi = builder.AddProject<Projects.BooksApp_Host>("booksapp-host")
                        .WithReference(postgresdb)
                        .WithReference(redis);

// --- Inject Stripe Secrets (Development Only) ---
if (builder.Environment.IsDevelopment())
{
    backendApi.WithEnvironment("Stripe:SecretKey", builder.Configuration["Stripe:SecretKey"]);
    backendApi.WithEnvironment("Stripe:WebhookSecret", builder.Configuration["Stripe:WebhookSecret"]);
}

builder.Build().Run();
