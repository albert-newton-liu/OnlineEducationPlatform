using OnlineEducation.Data.Dao;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Repository;
using OnlineEducation.Core;
using OnlineEducation.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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


builder.Services.AddScoped<IUserCoreService, UserCoreService>();
builder.Services.AddScoped<ILessonCoreSerice, LessonCoreSerice>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILessonService, LessonService>();

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseCors("AllowSpecificOrigin");

app.Run();

