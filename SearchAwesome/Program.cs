using Microsoft.EntityFrameworkCore;
using SearchAwesome;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<SearchDbContext>(opts => opts
    .UseNpgsql("database=searchawesome; host=localhost; username=postgres; password=test")
    .UseSnakeCaseNamingConvention());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/api/reseed", async (SearchDbContext context) =>
{
    await context.Users.ExecuteDeleteAsync();
    context.Users.AddRange(
        new User()
        {
            MaId = "ma_K5006",
            DaId = "da_SEK_001",
            Name = "David Lencoch",
            DisplayName = "David",
            Email = "lencoch_david@company.com",
        }, new User()
        {
            MaId = "ma_K5008",
            DaId = "da_SEK_024",
            Name = "Dávid Kováč",
            DisplayName = "David K",
            Email = "zbgr8@company.com",
        }, new User()
        {
            MaId = "ma_K5105",
            DaId = "da_SEK_044",
            Name = "Pater Rezon",
            DisplayName = "petuk",
            Email = "zbgr8@other.company.com",
        });
    await context.SaveChangesAsync();
});

app.Run();
