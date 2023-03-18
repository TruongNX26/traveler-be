﻿using Data;
using Data.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Location;
using Service.Models.Tag;
using Service.Pagination;
using Service.Results;
using Shared;

namespace Service.Implementations;

public class LocationService : BaseService, ILocationService
{
    private readonly IMapper _mapper;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ILogger<LocationService> _logger;

    public LocationService(IUnitOfWork unitOfWork, IMapper mapper, ICloudStorageService cloudStorageService,
        ILogger<LocationService> logger) : base(unitOfWork)
    {
        _mapper = mapper;
        _cloudStorageService = cloudStorageService;
        _logger = logger;
    }

    public async Task<Result<LocationViewModel>> Create(LocationCreateModel model)
    {
        var entity = _unitOfWork.Repo<Location>().Add(_mapper.Map<Location>(model));
        entity.CreatedAt = DateTimeHelper.VnNow();

        _unitOfWork.Repo<LocationTag>().AddRange(
            model.Tags.Select(id => new LocationTag()
                {
                    LocationId = entity.Id,
                    TagId = id
                }
            )
        );
        await _unitOfWork.SaveChangesAsync();

        // result
        var tags = await _unitOfWork.Repo<Tag>().Query().Where(e => model.Tags.Contains(e.Id)).ToListAsync();
        var viewModel = _mapper.Map<LocationViewModel>(entity);
        viewModel.Tags = _mapper.Map<List<TagViewModel>>(tags);
        return viewModel;
    }

    public async Task<Result<LocationViewModel>> Update(Guid id, LocationUpdateModel model)
    {
        var entity = _unitOfWork.Repo<Location>()
            .TrackingQuery()
            .Include(e => e.LocationTags)
            .FirstOrDefault(e => e.Id == id);

        if (entity is null) return Error.NotFound();

        if (model.Name != null) entity.Name = model.Name;
        if (model.Country != null) entity.Country = model.Country;
        if (model.City != null) entity.City = model.City;
        if (model.Address != null) entity.Address = model.Address;
        if (model.Longitude != null) entity.Longitude = model.Longitude.Value;
        if (model.Latitude != null) entity.Latitude = model.Latitude.Value;
        if (model.Description != null) entity.Description = model.Description;

        if (model.Tags != null)
            entity.LocationTags = model.Tags.Select(t =>
                new LocationTag()
                {
                    LocationId = entity.Id,
                    TagId = t
                }
            ).ToList();

        await _unitOfWork.SaveChangesAsync();

        var tags = await _unitOfWork.Repo<Location>().Query()
            .Where(e => e.Id == id)
            .SelectMany(e => e.LocationTags)
            .Select(lt => lt.Tag)
            .ToListAsync();

        var viewModel = _mapper.Map<LocationViewModel>(entity);
        viewModel.Tags = _mapper.Map<List<TagViewModel>>(tags);
        return viewModel;
    }

    public async Task<Result> Delete(Guid id)
    {
        var entity = _unitOfWork.Repo<Location>().FirstOrDefault(e => e.Id == id);
        if (entity is null) return Error.NotFound();

        _unitOfWork.Repo<Location>().Remove(entity);

        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<LocationViewModel>> Find(Guid id)
    {
        var entity = await _unitOfWork
            .Repo<Location>()
            .Query()
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Country,
                e.City,
                e.Address,
                e.Longitude,
                e.Latitude,
                e.Description,
                Tags = e.LocationTags.Select(lt => lt.Tag).ToList(),
            })
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entity is null) return Error.NotFound();

        var viewModel = _mapper.Map<LocationViewModel>(entity);
        viewModel.Tags = _mapper.Map<List<TagViewModel>>(entity.Tags);

        return viewModel;
    }

    public async Task<Result<AttachmentViewModel>> CreateAttachment(Guid locationId, AttachmentCreateModel model)
    {
        await using var transaction = _unitOfWork.BeginTransaction();

        try
        {
            if (!_unitOfWork.Repo<Location>().Any(e => e.Id == locationId))
                return Error.NotFound();

            var attachment = _unitOfWork.Repo<Attachment>().Add(new Attachment()
            {
                ContentType = model.ContentType,
                CreatedAt = DateTimeHelper.VnNow()
            });

            _unitOfWork.Repo<LocationAttachment>().Add(new LocationAttachment()
            {
                LocationId = locationId,
                AttachmentId = attachment.Id
            });

            await _unitOfWork.SaveChangesAsync();

            var uploadResult = await _cloudStorageService.Upload(attachment.Id, attachment.ContentType, model.Stream);

            if (!uploadResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            await transaction.CommitAsync();

            return new AttachmentViewModel()
            {
                Id = attachment.Id,
                ContentType = attachment.ContentType,
                Url = uploadResult.Value
            };
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }
    }

    public async Task<Result> DeleteAttachment(Guid locationId, Guid attachmentId)
    {
        await using var transaction = _unitOfWork.BeginTransaction();

        try
        {
            var attachment = _unitOfWork.Repo<Attachment>().FirstOrDefault(a => a.Id == attachmentId);
            if (attachment is null) return Error.NotFound();

            var locationAttachment = _unitOfWork.Repo<LocationAttachment>()
                .FirstOrDefault(e => e.LocationId == locationId && e.AttachmentId == attachmentId);
            if (locationAttachment is null) return Error.NotFound();

            _unitOfWork.Repo<LocationAttachment>().Remove(locationAttachment);
            _unitOfWork.Repo<Attachment>().Remove(attachment);
            
            await _unitOfWork.SaveChangesAsync();

            var storageResult = await _cloudStorageService.Delete(attachment.Id);
            if (!storageResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            await transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }
    }

    public async Task<Result<List<AttachmentViewModel>>> ListAttachments(Guid id)
    {
        if (!_unitOfWork.Repo<Location>().Any(e => e.Id == id))
            return Error.NotFound();

        var attachments = await _unitOfWork.Repo<LocationAttachment>().Query()
            .Where(e => e.LocationId == id)
            .Select(e => e.Attachment)
            .ToListAsync();

        var viewModels = new List<AttachmentViewModel>();

        foreach (var attachment in attachments)
        {
            var result = await _cloudStorageService.GetMediaLink(attachment.Id);
            viewModels.Add(new AttachmentViewModel()
            {
                Id = attachment.Id,
                ContentType = attachment.ContentType,
                Url = result.IsSuccess ? result.Value : null
            });
        }

        return viewModels;
    }

    public async Task<Result<PaginationModel<LocationViewModel>>> Filter(LocationFilterModel model)
    {
        IQueryable<Location> locations;
        if (model.Tags is { Count: > 0 })
            locations = _unitOfWork
                .Repo<LocationTag>()
                .Query()
                .Where(e => model.Tags.Contains(e.TagId))
                .Select(e => e.Location);
        else
            locations = _unitOfWork.Repo<Location>().Query();

        locations.Include(e => e.LocationTags).ThenInclude(lt => lt.Tag);
        
        var paginationModel = await locations.Paging(
            page: model.Page,
            size: model.Size,
            orderBy: nameof(Location.CreatedAt),
            PaginationExtensions.Order.ASC);

        return paginationModel.Map<LocationViewModel>(x =>
        {
            var viewModel = _mapper.Map<LocationViewModel>(x);
            viewModel.Tags = _mapper.Map<List<TagViewModel>>(x.LocationTags.Select(lt => lt.Tag));
            return viewModel;
        });
    }
}