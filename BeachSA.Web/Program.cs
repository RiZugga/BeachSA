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



/* Configuración de Autenticación mediante Cookies */

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(config =>
    {
        // Nombre de la cookie
        config.Cookie.Name = "BeachSA.Auth";

        // Ruta del formulario de inicio de sesión
        config.LoginPath = "/Account/Login";

        // La cookie no puede ser leída por JavaScript
        config.Cookie.HttpOnly = true;

        // Tiempo máximo de autenticación
        config.ExpireTimeSpan = TimeSpan.FromMinutes(8);

        // Ruta para accesos no autorizados por permisos
        config.AccessDeniedPath = "/Account/AccessDenied";

        // No renovar automáticamente la duración
        config.SlidingExpiration = false;
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


// Configuración de rutas
app.UseRouting();

// Habilitar autenticación mediante cookies
app.UseAuthorization();

// Habilitar autorización de usuarios
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
