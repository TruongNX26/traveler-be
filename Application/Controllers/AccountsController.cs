﻿using Application.Commons;
using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Account;
using Service.Models.Attachment;
using Shared.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers;

[Route("accounts")]
public class AccountsController : ApiController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [SwaggerOperation(description: "File size < 5MB")]
    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponsePayload), StatusCodes.Status400BadRequest)]
    [Authorize]
    [HttpPut("avatar")]
    public async Task<IActionResult> UpdateAvatar(IFormFile file)
    {
        var validateResult = FileHelper.ValidateImageFile(file);
        if (!validateResult.IsSuccess) return OnError(validateResult.Error);

        var result = await _accountService.UpdateAvatar(CurrentUser.Id, file.ContentType, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(AvatarViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponsePayload), StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpGet("avatar")]
    public IActionResult GetAvatar()
    {
        var result = _accountService.GetAvatar(CurrentUser.Id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(ProfileViewModel), StatusCodes.Status200OK)]
    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var result = _accountService.GetProfile(CurrentUser.Id, CurrentUser.Role);
        return result.Match(Ok, OnError);
    }
}