using EcaInventoryApi.Common;

namespace EcaInventoryApi.Middleware
{
	public class ErrorHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ErrorHandlingMiddleware> _logger;

		public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch(BadRequestException brEx)
			{
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsJsonAsync(new { error = brEx.Message });
			}
			catch (NotFoundException nfEx)
			{
				context.Response.StatusCode = StatusCodes.Status404NotFound;
				await context.Response.WriteAsJsonAsync(new { error = nfEx.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception");
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
			}
		}
	}
}