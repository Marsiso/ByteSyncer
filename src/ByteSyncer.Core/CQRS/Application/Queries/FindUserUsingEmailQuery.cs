using ByteSyncer.Data.EF;
using ByteSyncer.Domain.Application.Models;
using ByteSyncer.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ByteSyncer.Core.CQRS.Application.Queries
{
    public record FindUserUsingEmailQuery(string? Email) : IRequest<FindUserUsingEmailQueryResult>;

    public class FindUserUsingEmailQueryHandler : IRequestHandler<FindUserUsingEmailQuery, FindUserUsingEmailQueryResult>
    {
        private static readonly Func<DataContext, string, Task<User?>> Query = EF.CompileAsyncQuery((DataContext context, string email) =>
            context.Users.AsNoTracking()
                         .Include(user => user.UserRoles)
                         .ThenInclude(userRole => userRole.Role)
                         .Include(user => user.RootFolder)
                         .ThenInclude(rootFolder => rootFolder!.Children)
                         .Include(user => user.RootFolder)
                         .ThenInclude(rootFolder => rootFolder!.Files)
                         .Include(user => user.UserCreatedBy)
                         .Include(user => user.UserUpdatedBy)
                         .SingleOrDefault(user => user.Email == email));

        private readonly DataContext _context;
        private readonly ILogger<FindUserUsingEmailQueryHandler> _logger;

        public FindUserUsingEmailQueryHandler(DataContext context, ILogger<FindUserUsingEmailQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<FindUserUsingEmailQueryResult> Handle(FindUserUsingEmailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request is null)
                {
                    return new FindUserUsingEmailQueryResult(FindUserUsingEmailQueryResultType.BadRequest, default, new NullReferenceException("Request can not be a null-reference object."));
                }

                User? originalUser = default;

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    originalUser = await Query(_context, request.Email);
                }

                if (originalUser is null)
                {
                    return new FindUserUsingEmailQueryResult(FindUserUsingEmailQueryResultType.UserNotFound, default, new EntityNotFoundException(request.Email, nameof(User)));
                }

                return new FindUserUsingEmailQueryResult(FindUserUsingEmailQueryResultType.UserFound, originalUser, default);
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogError(exception.Message);

                return new FindUserUsingEmailQueryResult(FindUserUsingEmailQueryResultType.OperationCanceled, default, exception);
            }
            catch (TimeoutException exception)
            {
                _logger.LogError(exception.Message);

                return new FindUserUsingEmailQueryResult(FindUserUsingEmailQueryResultType.OperationTimedout, default, exception);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

                return new FindUserUsingEmailQueryResult(FindUserUsingEmailQueryResultType.InternalServerError, default, exception);
            }
        }
    }

    public record FindUserUsingEmailQueryResult(FindUserUsingEmailQueryResultType ResultType, User? Result, Exception? Exception) : RequestResultTypeClassBase<FindUserUsingEmailQueryResultType, User>(ResultType, Result, Exception);

    public enum FindUserUsingEmailQueryResultType
    {
        UserFound,
        UserNotFound,
        OperationCanceled,
        OperationTimedout,
        BadRequest,
        InternalServerError
    }
}
