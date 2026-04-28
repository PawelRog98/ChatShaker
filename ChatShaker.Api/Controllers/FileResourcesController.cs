using ChatShaker.Api.Helpers;
using ChatShaker.Application.Files.Commands;
using ChatShaker.Application.Files.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatShaker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FileResourcesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FileResourcesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload([FromBody] UploadedFileDto fileData, CancellationToken cancellationToken)
    {
        if(fileData.File == null ||  fileData.File.Length == 0)
            return BadRequest("File is empty");
        
        var result = await _mediator.Send(new UploadFileCommand(fileData), cancellationToken);
        
        return ApiResponse.Ok(result, "Image saved");
    }

    [HttpGet("download/{publicId}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DownloadFileQuery(publicId), cancellationToken);

        if (result == null)
            return ApiResponse.NotFound();

        return File(result.FileStream, result.ContentType ?? "application/octet-stream", result.FileName);
    }
}