namespace ChatShaker.Api.Helpers;

public class ErrorResponse : Response<object>
{
    public ErrorResponse()
    {
        Success = false;
    }
    
    public  ErrorResponse(string message) : base(message)
    {
        Success = false;
    }

    public ErrorResponse(string message, string[] errors) : base(message, errors)
    {
        Success = false;
    }

    public ErrorResponse(string message, string[] errors, dynamic metaData) : base(message, errors)
    {
        Success = false;
        MetaData = metaData;
    }

    public ErrorResponse(string message, dynamic metaData) : base(message)
    {
        Success = false;
        MetaData = metaData;
    }
}