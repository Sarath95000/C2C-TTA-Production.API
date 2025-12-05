using Microsoft.EntityFrameworkCore;
using TTA_API.Data;
using TTA_API.Services;

var builder = WebApplication.CreateBuilder(args);


// *** STEP 1: ADD DBCONTEXT ***
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// ****************************

// *** STEP 1: DEFINE A CORS POLICY ***
builder.Services.AddCors(options =>
{
    options.AddPolicy("NetlifyPolicy", policy =>
    {
        //policy.WithOrigins("http://localhost:3000")
        policy.WithOrigins("https://c2c-tta-app.netlify.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // only if you need cookies/auth
    });
});
// **********************************



// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILoginEvents, LoginEventService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseSwagger();
//app.UseSwaggerUI();

app.UseHttpsRedirection();

// *** STEP 2: USE THE CORS POLICY ***
// This MUST be placed after UseHttpsRedirection and before UseAuthorization
app.UseCors("NetlifyPolicy");
// **********************************

app.UseAuthorization();

app.MapControllers();

app.Run();
