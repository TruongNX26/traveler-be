﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service.Interfaces;
using Service.Models.Traveler;

namespace Application.Controllers;

[Route("travelers")]
public class TravelersController : ApiController
{
    private readonly ITravelerService _travelerService;
    private readonly ILogger<TravelersController> _logger;

    public TravelersController(ITravelerService travelerService, ILogger<TravelersController> logger)
    {
        _travelerService = travelerService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(TravelerRegistrationModel model)
    {
        if (!model.Phone.StartsWith('+')) model.Phone = '+' + model.Phone;
        var result = await _travelerService.Register(model);
        return result.Match(Ok, OnError);
    }

    [HttpGet("profile")]
    public IActionResult GetProfile(Guid id)
    {
        var result = _travelerService.GetProfile(id);

        _logger.LogInformation("{Message}", JsonConvert.SerializeObject(result.Value));

        return result.Match(Ok, OnError);
    }
}