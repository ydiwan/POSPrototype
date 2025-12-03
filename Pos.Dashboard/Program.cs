var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// HttpClient for calling your POS API
builder.Services.AddHttpClient("PosApi", client =>
{
    // IMPORTANT: use your API's HTTP base URL and port
    client.BaseAddress = new Uri("http://localhost:5171/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();


