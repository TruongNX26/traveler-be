﻿using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Activity;
using Service.Models.IncurredCost;
using Service.Models.TourGroup;
using Service.Models.User;

namespace Application.Controllers;

[Route("tour-groups")]
public class TourGroupsController : ApiController
{
    private readonly ITourGroupService _tourGroupService;
    private readonly IIncurredCostService _incurredCostService;

    public TourGroupsController(ITourGroupService tourGroupService,
        IIncurredCostService incurredCostService)
    {
        _tourGroupService = tourGroupService;
        _incurredCostService = incurredCostService;
    }

    /// <summary>
    /// Get a tour group
    /// </summary>
    [ProducesResponseType(typeof(List<TourGroupViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _tourGroupService.Get(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all travelers and tour guide of a group
    /// </summary>
    [ProducesResponseType(typeof(List<UserViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> ListMembers([FromRoute] Guid id)
    {
        var result = await _tourGroupService.ListMembers(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all activities of a tour group
    /// </summary>
    [ProducesResponseType(typeof(List<ActivityViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/activities")]
    public async Task<IActionResult> ListActivities(Guid id)
    {
        var result = await _tourGroupService.ListActivities(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all Incurred Costs in group
    /// </summary>
    [ProducesResponseType(typeof(List<IncurredCostViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/incurred-costs")]
    public async Task<IActionResult> ListIncurredCosts(Guid id)
    {
        var result = await _incurredCostService.ListAll(id);
        return result.Match(Ok, OnError);
    }
}