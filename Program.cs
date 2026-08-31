using HotelReservation.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


// Authantication
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
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!))
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewHotels", policy =>
    {
        policy.RequireClaim("Permission", "ViewHotels");
    });

    options.AddPolicy("CreateHotel", policy =>
    {
        policy.RequireClaim("Permission", "CreateHotel");
    });

    options.AddPolicy("UpdateHotel", policy =>
    {
        policy.RequireClaim("Permission", "UpdateHotel");
    });

    options.AddPolicy("DeleteHotel", policy =>
    {
        policy.RequireClaim("Permission", "DeleteHotel");
    });

    options.AddPolicy("ViewRooms", policy =>
    {
        policy.RequireClaim("Permission", "ViewRooms");
    });

    options.AddPolicy("CreateRoom", policy =>
    {
        policy.RequireClaim("Permission", "CreateRoom");
    });

    options.AddPolicy("UpdateRoom", policy =>
    {
        policy.RequireClaim("Permission", "UpdateRoom");
    });

    options.AddPolicy("DeleteRoom", policy =>
    {
        policy.RequireClaim("Permission", "DeleteRoom");
    });

    options.AddPolicy("ViewReservations", policy =>
    {
        policy.RequireClaim("Permission", "ViewReservations");
    });

    options.AddPolicy("ReservationByid", policy =>
    {
        policy.RequireClaim("Permission", "ReservationByid");
    });

    options.AddPolicy("CreateReservation", policy =>
    {
        policy.RequireClaim("Permission", "CreateReservation");
    });

    options.AddPolicy("UpdateReservation", policy =>
    {
        policy.RequireClaim("Permission", "UpdateReservation");
    });

    options.AddPolicy("CancelReservation", policy =>
    {
        policy.RequireClaim("Permission", "CancelReservation");
    });

    options.AddPolicy("CreatePayment", policy =>
    {
        policy.RequireClaim("Permission", "CreatePayment");
    });

    options.AddPolicy("ViewPayments", policy =>
    {
        policy.RequireClaim("Permission", "ViewPayments");
    });

    options.AddPolicy("ViewAllPayments", policy =>
    {
        policy.RequireClaim("Permission", "ViewAllPayments");
    });

    options.AddPolicy("CreateReview", policy =>
    {
        policy.RequireClaim("Permission", "CreateReview");
    });

    options.AddPolicy("ViewReviews", policy =>
    {
        policy.RequireClaim("Permission", "ViewReviews");
    });
    options.AddPolicy("ViewUsers", policy =>
    {
        policy.RequireClaim("Permission", "ViewUsers");
    });
});
//  Call with DBContext
builder.Services.AddDbContext<HotelReservationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
