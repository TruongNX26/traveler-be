﻿using System.Reflection;
using Data;
using Mapster;
using MapsterMapper;
using Microsoft.OpenApi.Models;
using Service.Implementations;
using Service.Interfaces;
using Shared.Settings;

namespace Application.Configurations
{
    public static class AppConfiguration
    {
        public static void AddSettings(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
            builder.Services.Configure<VnPaySettings>(builder.Configuration.GetSection("VnPaySettings"));
        }

        public static void AddDependenceInjection(this IServiceCollection services)
        {
            // Add Mapper
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();
            
            // Add Services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICloudMessagingService, CloudMessagingService>();
            services.AddScoped<IVnPayRequestService, VnPayRequestService>();
            services.AddScoped<IVnPayResponseService, VnPayResponseService>();
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Traveler Service Interface", Description = "APIs for Traveler Application",
                        Version = "v1"
                    });
                c.DescribeAllParametersInCamelCase();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });
        }
    }
}