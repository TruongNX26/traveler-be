﻿using System.Data;
using System.IO.Compression;
using Data.Enums;
using ExcelDataReader;

namespace Service.ImportHelpers;

public static class TourImportHelper
{
    /// <summary>
    /// Extract TourModel from zip archive
    /// </summary>
    public static TourModel ReadTourArchive(Stream zipData)
    {
        var zipArchive = new ZipArchive(zipData);

        var tour = _readTourExcel(zipArchive);
        tour.Thumbnail = _readThumbnail(zipArchive);
        tour.Images = _readImages(zipArchive);

        return tour;
    }

    /// <summary>
    /// Read Tour data from excel file
    /// </summary>
    private static TourModel _readTourExcel(ZipArchive zipArchive)
    {
        var tourExcelFile = zipArchive.Entries.FirstOrDefault(entry => entry.Name.Equals("Tour.xlsx"));
        if (tourExcelFile is null) throw new Exception("Tour.xlsx not found");

        var tourExcelData = new MemoryStream();
        tourExcelFile.Open().CopyTo(tourExcelData);

        // Create excel reader
        using var reader = ExcelReaderFactory.CreateReader(tourExcelData);

        reader.Read(); // skip header
        reader.Read(); // tour data
        var tour = new TourModel()
        {
            Id = Guid.Parse(reader.GetString(0)),
            Title = reader.GetString(1),
            Departure = reader.GetString(2),
            Destination = reader.GetString(3),
            Duration = reader.GetString(4),
            Type = Enum.Parse<TourType>(reader.GetString(5)),
            Description = reader.GetString(6),
            Policy = reader.GetString(7)
        };

        // Read schedules
        reader.NextResult();
        tour.Schedules = _readSchedules(reader);

        return tour;
    }

    /// <summary>
    /// Read Tour schedules from excel file
    /// </summary>
    private static IEnumerable<ScheduleModel> _readSchedules(IDataReader reader)
    {
        var schedules = new List<ScheduleModel>();

        reader.Read(); // skip header
        while (reader.Read())
        {
            schedules.Add(new ScheduleModel()
            {
                Sequence = (int)reader.GetDouble(0),
                Description = reader.GetString(1),
                Longitude = IsNull(reader.GetValue(2)) ? null : reader.GetDouble(2),
                Latitude = IsNull(reader.GetValue(3)) ? null : reader.GetDouble(3),
                DayNo = (int)reader.GetDouble(4),
                Vehicle = IsNull(reader.GetValue(5)) ? null : Enum.Parse<Vehicle>(reader.GetString(5))
            });
        }

        return schedules;
    }

    /// <summary>
    /// Read tour thumbnail image
    /// </summary>
    private static ImageModel _readThumbnail(ZipArchive zipArchive)
    {
        var image = zipArchive.Entries
            .Where(entry => entry.Name.StartsWith("thumbnail"))
            .Select(entry =>
            {
                var img = new ImageModel()
                {
                    Extension = Path.GetExtension(entry.Name),
                    Data = new MemoryStream()
                };
                entry.Open().CopyTo(img.Data);
                return img;
            })
            .FirstOrDefault();

        if (image is null) throw new Exception("Thumbnail not found.");
        return image;
    }

    /// <summary>
    /// Read carousel images
    /// </summary>
    private static IEnumerable<ImageModel> _readImages(ZipArchive zipArchive)
    {
        return zipArchive.Entries
            .Where(entry => !string.IsNullOrEmpty(entry.Name) && entry.FullName.StartsWith("images/"))
            .Select(entry =>
            {
                var img = new ImageModel()
                {
                    Extension = Path.GetExtension(entry.Name),
                    Data = new MemoryStream()
                };
                entry.Open().CopyTo(img.Data);
                return img;
            });
    }

    // Checking if property is null
    private static bool IsNull(this object obj) => ReferenceEquals(obj, null);
}

#region Models

public class ImageModel
{
    public string Extension { get; set; } = null!;
    public MemoryStream Data { get; set; } = null!;
}

public class TourModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Departure { get; set; } = null!;
    public string Destination { get; set; } = null!;
    public string Duration { get; set; } = null!;
    public TourType Type { get; set; }
    public string Description { get; set; } = null!;
    public string Policy { get; set; } = null!;
    public IEnumerable<ScheduleModel> Schedules { get; set; } = null!;
    public ImageModel Thumbnail { get; set; } = null!;
    public IEnumerable<ImageModel> Images { get; set; } = null!;
}

public class ScheduleModel
{
    public int Sequence { get; set; }
    public string Description { get; set; } = null!;
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public int DayNo { get; set; }
    public Vehicle? Vehicle { get; set; }
}

#endregion