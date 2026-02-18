using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using WorkplaceTasks.API.Data;
using WorkplaceTasks.API.Models;
using WorkplaceTasks.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
//Serviços e Configurações)
// ==========================================

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Base de Dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Injeção de Dependências Services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();

// Ativar o CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:5173", "http://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configuração JWT
var jwtSecret = builder.Configuration["JwtSettings:Secret"]!.Trim();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.UseSecurityTokenValidators = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("[Auth] JWT Validation Error: " + context.Exception);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();


// ==========================================
// 2. Construção
// ==========================================
var app = builder.Build();


// ==========================================
// 3. PIPELINE 
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("ReactPolicy");

// Tratamento de erros
app.UseMiddleware<WorkplaceTasks.API.Middleware.ExceptionMiddleware>();

// Segurança
app.UseAuthentication();
app.UseAuthorization();

// Controladores
app.MapControllers();


// ==========================================
// 4. Carregar a DB
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), Username = "admin", PasswordHash = "admin123", Role = "admin" },
            new User { Id = Guid.NewGuid(), Username = "manager", PasswordHash = "manager123", Role = "manager" },
            new User { Id = Guid.NewGuid(), Username = "member", PasswordHash = "member123", Role = "member" }
        );
        context.SaveChanges();
    }
}

app.Run();