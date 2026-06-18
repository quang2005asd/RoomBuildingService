namespace RoomBuildingService.Api.Middlewares;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RoomBuildingService.Core.Exceptions;
public class GlobalExceptionHandler : IExceptionHandler
{
public async ValueTask<bool> TryHandleAsync(
HttpContext context,
Exception   exception,
CancellationToken ct)
{
var (statusCode, message) = exception switch
{
NotFoundException    ex => (StatusCodes.Status404NotFound,  ex.Message),
BusinessRuleException ex => (StatusCodes.Status400BadRequest, ex.Message),
_                       => (StatusCodes.Status500InternalServerError,
"Lỗi hệ thống, vui lòng thử lại sau.")
};
    context.Response.StatusCode  = statusCode;
    context.Response.ContentType = "application/json";

    var problem = new ProblemDetails
    {
        Status = statusCode,
        Title  = statusCode switch
        {
            404 => "Không tìm thấy",
            400 => "Dữ liệu không hợp lệ",
            _   => "Lỗi hệ thống"
        },
        Detail = message
    };

    await context.Response.WriteAsJsonAsync(problem, ct);
    return true;
}
}