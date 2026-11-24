using BeachSA.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

/* Configuración de MVC */
builder.Services.AddControllersWithViews();

/* Acceso al Contexto HTTP */
builder.Services.AddHttpContextAccessor();

/* Configuración del Servicio para Consumir la API */
builder.Services.AddHttpClient<ApiClient>(client =>
{
    // Obtener la dirección de la API desde appsettings.json
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

    // Configurar la dirección si ya está definida
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
});

// Si en UsuarioApiService usas _httpClientFactory.CreateClient("BeachApi"), mantén este registro también:
builder.Services.AddHttpClient("BeachApi", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
});

builder.Services.AddScoped<IUsuarioApiService, UsuarioApiService>();
builder.Services.AddScoped<IAuditoriaApiService, AuditoriaApiService>();
builder.Services.AddScoped<RolApiService>();

/* Configuración de Autenticación mediante Cookies */
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(config =>
    {
        config.Cookie.Name = "BeachSA.Auth";
        config.LoginPath = "/Account/Login";
        config.Cookie.HttpOnly = true;
        config.ExpireTimeSpan = TimeSpan.FromMinutes(8);
        config.AccessDeniedPath = "/Account/AccessDenied";
        config.SlidingExpiration = false;
    });

builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configuración de rutas
app.UseRouting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Sesión (después de UseRouting y UseAuthentication)
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();