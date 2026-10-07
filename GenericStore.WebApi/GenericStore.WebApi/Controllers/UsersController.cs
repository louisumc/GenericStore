using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Stores;
using GenericStore.Application.DTOs.Users;
using GenericStore.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace GenericStore.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    private readonly IStoreService _storeService;

    public UsersController(IUserService userService, IStoreService storeService)
    {
        _service = userService;
        _storeService = storeService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var response = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var response = await _service.GetByIdAsync(id, ct);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var response = await _service.GetAllAsync(ct);
        return Ok(response);
    }

    [HttpGet("{userId:guid}/stores")]
    [ProducesResponseType(typeof(IReadOnlyList<StoreResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStores(Guid userId, CancellationToken ct)
    {
        var response = await _storeService.GetByUserIdAsync(userId, ct);
        return Ok(response);
    }
}