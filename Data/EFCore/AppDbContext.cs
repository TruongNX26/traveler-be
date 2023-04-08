﻿using System.Reflection;
using Data.EFCore.Configurations;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Data.EFCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.ConfigureEnums();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureEntities();

        // Seeds
        modelBuilder.Entity<Traveler>().HasData(_fromSeed<Traveler>("travelers.json"));
        modelBuilder.Entity<TourGuide>().HasData(_fromSeed<TourGuide>("tour-guides.json"));
        modelBuilder.Entity<Manager>().HasData(_fromSeed<Manager>("managers.json"));
        modelBuilder.Entity<Location>().HasData(_fromSeed<Location>("locations.json"));

        modelBuilder.Entity<TourGroup>().HasData(_fromSeed<TourGroup>("tour-groups.json"));
        modelBuilder.Entity<Tour>().HasData(_fromSeed<Tour>("tours.json"));
        modelBuilder.Entity<TravelerInTourGroup>()
            .HasData(_fromSeed<TravelerInTourGroup>("traveler-in-tour-group.json"));
    }


    // PRIVATE
    private static IEnumerable<T> _fromSeed<T>(string fileName)
    {
        var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var seedPath = Path.Combine(projectPath, "EFCore", "Seeds", fileName);
        return JsonConvert.DeserializeObject<IEnumerable<T>>(File.ReadAllText(seedPath))!;
    }
}