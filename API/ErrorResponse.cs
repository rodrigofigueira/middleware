namespace API;

public record ErrorResponse(int StatusCode, string Message, string ErrorDetails = null!) { }