using ByteSyncer.Data.EF;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ByteSyncer.Core.Application.Queries
{
    public record EmailExistsQuery(string? Email) : IRequest<EmailExistsQueryResult>;


    public class EmailExistsQueryHandler : IRequestHandler<EmailExistsQuery, EmailExistsQueryResult>
    {
        private static readonly Func<DataContext, string, Task<bool>> Query = EF.CompileAsyncQuery((DataContext context, string email) =>
            context.Users.AsNoTracking()
                         .Any(user => user.Email == email));

        private readonly DataContext _context;
        private readonly ILogger<EmailExistsQueryHandler> _logger;

        public EmailExistsQueryHandler(DataContext context, ILogger<EmailExistsQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EmailExistsQueryResult> Handle(EmailExistsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request is null)
                {
                    return new EmailExistsQueryResult(EmailExistsQueryResultType.InternalServerError, false, new NullReferenceException("Email exists query can not be a null-reference object."));
                }

                bool emailExists = false;
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    emailExists = await Query(_context, request.Email);
                }

                return new EmailExistsQueryResult(EmailExistsQueryResultType.Succeded, emailExists, default);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

                return new EmailExistsQueryResult(EmailExistsQueryResultType.InternalServerError, false, exception);
            }
        }
    }

    public record EmailExistsQueryResult(EmailExistsQueryResultType Result, bool Exists, Exception? exception);

    public enum EmailExistsQueryResultType
    {
        Succeded,
        Canceled,
        Timedout,
        InternalServerError
    }
}
