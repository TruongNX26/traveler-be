﻿using System.Data;
using Data.EFCore;
using Data.Entities;
using Data.Enums;
using ExcelDataReader;
using Service.Interfaces;
using Service.Models.Tour;
using Service.Models.Trip;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class DataImportService : BaseService, IDataImportService
{
    private readonly ITourService _tourService;

    static DataImportService()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    public DataImportService(UnitOfWork unitOfWork, ITourService tourService) : base(unitOfWork)
    {
        _tourService = tourService;
    }

    /// <summary>
    /// Import Trip
    /// </summary>

    #region Trip

    public Task<Result<TripViewModel>> ImportTrip(Stream fileStream)
    {
        throw new NotImplementedException();
    }

    #endregion


    /// <summary>
    /// Import Tour
    /// </summary>

    #region Tour

    public async Task<Result<TourDetailsViewModel>> ImportTour(Stream fileStream)
    {
        try
        {
            using var reader = ExcelReaderFactory.CreateReader(fileStream);
            var tour = _readTour(reader);

            if (await UnitOfWork.Tours.AnyAsync(e => e.Id == tour.Id))
                return Error.Conflict($"Tour '{tour.Id}' already exist.");

            UnitOfWork.Tours.Add(tour);
            await UnitOfWork.SaveChangesAsync();

            return await _tourService.GetDetails(tour.Id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
            return Error.Validation(e.Message);
        }
    }

    /// <summary>
    /// Read Tour data from excel file
    /// </summary>
    private static Tour _readTour(IDataReader reader)
    {
        reader.Read(); // skip header
        reader.Read(); // tour data
        var tour = new Tour()
        {
            Id = Guid.Parse(reader.GetString(0)),
            Title = reader.GetString(1),
            Departure = reader.GetString(2),
            Destination = reader.GetString(3),
            Duration = reader.GetString(4),
            Description = reader.GetString(6),
            Policy = reader.GetString(7),
            ThumbnailId = Guid.Parse(reader.GetString(8))
        };

        tour.Type = Enum.Parse<TourType>(reader.GetString(5));

        // Read images
        reader.NextResult();
        tour.TourCarousel = _readImages(reader);

        // Read schedules
        reader.NextResult();
        tour.Schedules = _readSchedules(reader);

        return tour;
    }

    /// <summary>
    /// Read Tour images from excel file
    /// </summary>
    private static ICollection<TourImage> _readImages(IDataReader reader)
    {
        var imageIds = new List<Guid>();

        reader.Read(); // skip header
        while (reader.Read())
        {
            imageIds.Add(Guid.Parse(reader.GetString(0)));
        }

        return imageIds.Select(imageId => new TourImage() { AttachmentId = imageId }).ToList();
    }

    /// <summary>
    /// Read Tour schedules from excel file
    /// </summary>
    private static ICollection<Schedule> _readSchedules(IDataReader reader)
    {
        var schedules = new List<Schedule>();

        reader.Read(); // skip header
        while (reader.Read())
        {
            var schedule = new Schedule()
            {
                Sequence = (int)reader.GetDouble(0),
                Description = reader.GetString(1),
                Longitude = ReferenceEquals(reader.GetValue(2), null) ? null : reader.GetDouble(2),
                Latitude = ReferenceEquals(reader.GetValue(3), null) ? null : reader.GetDouble(3),
                DayNo = (int)reader.GetDouble(4),
            };

            if (!ReferenceEquals(reader.GetString(5), null))
                schedule.Vehicle = Enum.Parse<Vehicle>(reader.GetString(5));

            schedules.Add(schedule);
        }

        return schedules;
    }

    #endregion
}