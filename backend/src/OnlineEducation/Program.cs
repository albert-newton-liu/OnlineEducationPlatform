using OnlineEducation.Data.Dao;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Repository;
using OnlineEducation.Core;
using OnlineEducation.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OnlineEducation.Utils;
using System.IdentityModel.Tokens.Jwt;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ILessonPageRepository, LessonPageRepository>();
builder.Services.AddScoped<ILessonPageElementRepository, LessonPageElementRepository>();

builder.Services.AddScoped<ITeacherScheduleRepository, TeacherScheduleRepository>();
builder.Services.AddScoped<IBookableSlotRepository, BookableSlotRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

builder.Services.AddScoped<IUserCoreService, UserCoreService>();
builder.Services.AddScoped<ILessonCoreSerice, LessonCoreSerice>();
builder.Services.AddScoped<IBookingCoreService, BookingCoreService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
builder.Services.AddSingleton<JwtSecurityTokenHandler>();


builder.Services.AddScoped<ITeacherBookSlotJob, TeacherBookSlotJob>();
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("teacherBookSlotJob");
    q.AddJob<TeacherBookSlotJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("teacherBookSlot-trigger")
        //  "0 0 0 ? * SUN *"
        .WithCronSchedule("0 0 0 ? * SUN *")); 
        // .WithCronSchedule("0 * * ? * *")); 
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);



// --- CORS Configuration Start ---
builder.Services.AddCors(options =>
{
    // Define a CORS policy named "AllowSpecificOrigin".
    // It's good practice to name your policies, especially if you have multiple.
    options.AddPolicy("AllowSpecificOrigin",
        policyBuilder =>
        {
            // IMPORTANT: In production, replace "http://localhost:5173" with your actual
            // production frontend domain (e.g., "https://yourfrontend.com").
            policyBuilder.WithOrigins("http://localhost:5173")
                         .AllowAnyHeader()
                         .AllowAnyMethod()
                         .AllowCredentials();
        });

});
// --- CORS Configuration End ---



builder.Services.AddSignalR();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub").RequireAuthorization();

app.Run();

