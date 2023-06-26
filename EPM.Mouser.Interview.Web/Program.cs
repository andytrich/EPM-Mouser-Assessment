using System.Text.Json.Serialization;
using EPM.Mouser.Interview.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddMvc().AddJsonOptions(opts =>
{
    var enumConverter = new JsonStringEnumConverter();
    opts.JsonSerializerOptions.Converters.Add(enumConverter);
});
builder.Services.SetupDiForWarehouse();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();

app.MapRazorPages();

app.Run();
