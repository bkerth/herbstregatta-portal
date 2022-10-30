using Herbstregatta.Data.Services;
using Herbstregatta.Data.Services.Data;
using Herbstregatta.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

// Database
var connectionString = builder.Configuration.GetConnectionString("HerbstregattaDbContext")
    ?? throw new InvalidOperationException("Connection string 'HerbstregattaDbContext' not found.");

builder.Services.AddDbContext<HerbstregattaDbContext>(
    options => options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 9, 3))));

builder.Services.AddScoped<IRaceRegistrationService, RaceRegistrationService>();
builder.Services.AddScoped<IRaceConfigurationService, RaceConfigurationService>();
builder.Services.AddScoped<IRaceSchedulingService, RaceSchedulingService>();
builder.Services.AddScoped<IStopwatchService, StopwatchService>();

builder.Services.AddSingleton<IBackupService, BackupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
