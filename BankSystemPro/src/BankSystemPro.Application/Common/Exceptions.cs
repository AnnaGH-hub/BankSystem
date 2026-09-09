namespace BankSystemPro.Application.Common
{
    // Base type for exceptions we deliberately throw for business-rule violations,
    // as opposed to unexpected bugs. The API's exception middleware maps these to
    // clean HTTP status codes instead of a generic 500.
    public abstract class AppException : Exception
    {
        protected AppException(string message) : base(message) { }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message) { }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
