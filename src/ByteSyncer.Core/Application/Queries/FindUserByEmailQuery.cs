using ByteSyncer.Data.EF;
using ByteSyncer.Domain.Application.Models;
using ByteSyncer.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ByteSyncer.Core.Application.Queries
{
    public record FindUserByEmailQuery(string? Email) : IRequest<FindUserByEmailQueryResult>;

    public class FindUserByEmailQueryHandler : IRequestHandler<FindUserByEmailQuery, FindUserByEmailQueryResult>
    {
        private static readonly Func<DataContext, string, Task<User?>> Query = EF.CompileAsyncQuery((DataContext context, string email) =>
            context.Users.AsNoTracking()
                         .Include(user => user.UserRoles)
                         !.ThenInclude(userRole => userRole.Role)
                         .Include(user => user.RootFolder)
                         !.ThenInclude(rootFolder => rootFolder!.Children)
                         .Include(user => user.RootFolder)
                         .ThenInclude(rootFolder => rootFolder!.Files)
                         .Include(user => user.UserCreatedBy)
                         .Include(user => user.UserUpdatedBy)
                         .SingleOrDefault(user => user.Email == email));

        private readonly DataContext _context;
        private readonly ILogger<FindUserByEmailQueryHandler> _logger;

        public FindUserByEmailQueryHandler(DataContext context, ILogger<FindUserByEmailQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<FindUserByEmailQueryResult> Handle(FindUserByEmailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request is null)
                {
                    return new FindUserByEmailQueryResult(FindUserByEmailQueryResultType.InternalServerError, default, new NullReferenceException("Find user by email query can not be a null-reference object."));
                }

                User? originalUser = default;

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    originalUser = await Query(_context, request.Email);
                }

                if (originalUser is null)
                {
                    return new FindUserByEmailQueryResult(FindUserByEmailQueryResultType.EntityNotFound, default, new EntityNotFoundException(request.Email, nameof(User)));
                }

                return new FindUserByEmailQueryResult(FindUserByEmailQueryResultType.Succeded, originalUser, default);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

                return new FindUserByEmailQueryResult(FindUserByEmailQueryResultType.InternalServerError, default, exception);
            }
        }
    }

    public record FindUserByEmailQueryResult(FindUserByEmailQueryResultType Result, User? User, Exception? Exception);

    public enum FindUserByEmailQueryResultType
    {
        Succeded,
        Canceled,
        Timedout,
        EntityNotFound,
        InternalServerError
    }
}
