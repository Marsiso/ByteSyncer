using AutoMapper;
using ByteSyncer.Core.CQRS.Application.Queries;
using ByteSyncer.Core.Helpers;
using ByteSyncer.Domain.Application.Models;
using ByteSyncer.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ByteSyncer.Core.CQRS.Application.Commands
{
    public record ValidateCredentialsCommand(string? Email, string? Password) : IRequest<ValidateCredentialsCommandResult>;

    public class ValidateCredentialsCommandHandler : IRequestHandler<ValidateCredentialsCommand, ValidateCredentialsCommandResult>
    {
        private readonly IValidator<ValidateCredentialsCommand> _validator;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<ValidateCredentialsCommandHandler> _logger;

        public ValidateCredentialsCommandHandler(IValidator<ValidateCredentialsCommand> validator, IMapper mapper, IMediator mediator, ILogger<ValidateCredentialsCommandHandler> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ValidateCredentialsCommandResult> Handle(ValidateCredentialsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request is null)
                {
                    return new ValidateCredentialsCommandResult(ValidateCredentialsCommandResultType.BadRequest, default, new NullReferenceException("Login command can not be a null-reference object."));
                }

                ValidationContext<ValidateCredentialsCommand> validationContext = new ValidationContext<ValidateCredentialsCommand>(request);
                ValidationResult validationResult = await _validator.ValidateAsync(validationContext, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return new ValidateCredentialsCommandResult(ValidateCredentialsCommandResultType.InvalidCredentials, default, new EntityValidationException("Login commad has validation errors.", ValidationResultHelpers.DistinctErrorsByProperty(validationResult)));
                }

                FindUserUsingEmailQuery query = _mapper.Map<FindUserUsingEmailQuery>(request);
                FindUserUsingEmailQueryResult queryResult = await _mediator.Send(query, cancellationToken);

                if (queryResult.ResultType != FindUserUsingEmailQueryResultType.UserFound)
                {
                    if (queryResult.ResultType == FindUserUsingEmailQueryResultType.UserNotFound)
                    {
                        return new ValidateCredentialsCommandResult(ValidateCredentialsCommandResultType.EntityNotFound, default, queryResult.Exception);
                    }

                    return new ValidateCredentialsCommandResult(ValidateCredentialsCommandResultType.InternalServerError, default, queryResult.Exception);
                }

                User? originalUser = queryResult.Result;

                return new ValidateCredentialsCommandResult(ValidateCredentialsCommandResultType.ValidCredentials, originalUser, default);
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogError(exception.Message);

                return new ValidateCredentialsCommandResult(ValidateCredentialsCommandResultType.OperationCanceled, default, exception);
            }
            catch (TimeoutException exception)
            {
                _logger.LogError(exception.Message);

                return new ValidateCredentialsCommandResult(ValidateCredentialsCommandResultType.OperationTimedout, default, exception);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

                return new ValidateCredentialsCommandResult(ValidateCredentialsCommandResultType.InternalServerError, default, exception);
            }
        }
    }

    public record ValidateCredentialsCommandResult(ValidateCredentialsCommandResultType ResultType, User? Result, Exception? Exception) : RequestResultTypeClassBase<ValidateCredentialsCommandResultType, User>(ResultType, Result, Exception);

    public enum ValidateCredentialsCommandResultType
    {
        BadRequest,
        ValidCredentials,
        InvalidCredentials,
        EntityNotFound,
        OperationCanceled,
        OperationTimedout,
        InternalServerError
    }
}
