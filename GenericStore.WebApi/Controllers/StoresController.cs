using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Products;
using GenericStore.Application.DTOs.Stores;
using GenericStore.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace GenericStore.Api.Controllers;

[ApiController]
[Route("api/stores")]
public class StoresController : ControllerBase
{
    private readonly IStoreService _service;
    private readonly IProductService _productService;

    public StoresController(IStoreService storeService, IProductService productService)
    {
        _service = storeService;
        _productService = productService;
    }


    [HttpPost]
    [ProducesResponseType(typeof(StoreResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStoreRequest request,
        CancellationToken ct)
    {
        var response = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var response = await _service.GetByIdAsync(id, ct);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StoreResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var response = await _service.GetAllAsync(ct);
        return Ok(response);
    }

    [HttpGet("{storeId:guid}/products")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProducts(Guid storeId, CancellationToken ct)
    {
        var response = await _productService.GetActiveByStoreIdAsync(storeId, ct);
        return Ok(response);
    }

    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _service.ActivateAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _service.DeactivateAsync(id, ct);
        return NoContent();
    }
}