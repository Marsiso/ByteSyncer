using AutoMapper;
using ByteSyncer.Core.Application.Queries;
using ByteSyncer.Core.Helpers;
using ByteSyncer.Domain.Application.Models;
using ByteSyncer.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ByteSyncer.Core.Application.Commands
{
    public record LoginCommand(string? Email, string? Password) : IRequest<LoginCommandResult>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResult>
    {
        private readonly IValidator<LoginCommand> _validator;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(IValidator<LoginCommand> validator, IMapper mapper, IMediator mediator, ILogger<LoginCommandHandler> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request is null)
                {
                    return new LoginCommandResult(LoginCommandResultType.InternalServerError, default, new NullReferenceException("Login command can not be a null-reference object."));
                }

                ValidationContext<LoginCommand> validationContext = new ValidationContext<LoginCommand>(request);
                ValidationResult validationResult = await _validator.ValidateAsync(validationContext, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return new LoginCommandResult(LoginCommandResultType.ValidationFailure, default, new EntityValidationException("Login commad has validation errors.", ValidationResultHelpers.DistinctErrorsByProperty(validationResult)));
                }

                FindUserByEmailQuery findUserByEmailQuery = _mapper.Map<FindUserByEmailQuery>(request);
                FindUserByEmailQueryResult findUserByEmailQueryResult = await _mediator.Send(findUserByEmailQuery, cancellationToken);

                if (findUserByEmailQueryResult.Result != FindUserByEmailQueryResultType.Succeded)
                {
                    if (findUserByEmailQueryResult.Result == FindUserByEmailQueryResultType.EntityNotFound)
                    {
                        return new LoginCommandResult(LoginCommandResultType.EntityNotFound, default, findUserByEmailQueryResult.Exception);
                    }

                    return new LoginCommandResult(LoginCommandResultType.InternalServerError, default, findUserByEmailQueryResult.Exception);
                }

                User? originalUser = findUserByEmailQueryResult.User;

                return new LoginCommandResult(LoginCommandResultType.Succeded, originalUser, default);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

                return new LoginCommandResult(LoginCommandResultType.InternalServerError, default, exception);
            }
        }
    }

    public record LoginCommandResult(LoginCommandResultType Result, User? User, Exception? Exception);

    public enum LoginCommandResultType
    {
        Succeded,
        EntityNotFound,
        ValidationFailure,
        InternalServerError
    }
}
