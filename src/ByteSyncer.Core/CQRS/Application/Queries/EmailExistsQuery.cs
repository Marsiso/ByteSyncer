using ByteSyncer.Data.EF;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ByteSyncer.Core.CQRS.Application.Queries
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
                    return new EmailExistsQueryResult(EmailExistsQueryResultType.InternalServerError, false, new NullReferenceException("Request can not be a null-reference object."));
                }

                bool emailExists = false;
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    emailExists = await Query(_context, request.Email);
                }

                EmailExistsQueryResult queryResult = new EmailExistsQueryResult(EmailExistsQueryResultType.EmailFound, emailExists, default);

                return queryResult;
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogError(exception.Message);

                return new EmailExistsQueryResult(EmailExistsQueryResultType.OperationCanceled, default, exception);
            }
            catch (TimeoutException exception)
            {
                _logger.LogError(exception.Message);

                return new EmailExistsQueryResult(EmailExistsQueryResultType.OperationTimedout, default, exception);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

                EmailExistsQueryResult queryResult = new EmailExistsQueryResult(EmailExistsQueryResultType.InternalServerError, false, exception);

                return queryResult;
            }
        }
    }

    public record EmailExistsQueryResult(EmailExistsQueryResultType ResultType, bool? Result, Exception? Exception) : RequestResultTypeStructBase<EmailExistsQueryResultType, bool>(ResultType, Result, Exception);

    public enum EmailExistsQueryResultType
    {
        EmailFound,
        EmailNotFound,
        OperationCanceled,
        OperationTimedout,
        InternalServerError
    }
}
