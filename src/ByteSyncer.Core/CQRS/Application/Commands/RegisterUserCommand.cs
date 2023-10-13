using AutoMapper;
using ByteSyncer.Core.CQRS.Files.Commands;
using ByteSyncer.Core.Helpers;
using ByteSyncer.Data.EF;
using ByteSyncer.Domain.Application.Models;
using ByteSyncer.Domain.Contracts;
using ByteSyncer.Domain.Exceptions;
using ByteSyncer.Domain.Files.Models;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ByteSyncer.Core.CQRS.Application.Commands
{
    public record RegisterUserCommand(string? Email, string? GivenName, string? FamilyName, string? Password, string? PasswordRepeat) : IRequest<RegisterUserCommandResult>;

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserCommandResult>
    {
        private readonly DataContext _context;
        private readonly IValidator<RegisterUserCommand> _validator;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPasswordProtector _passwordProtector;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(DataContext context, IValidator<RegisterUserCommand> validator, IMapper mapper, IMediator mediator, IPasswordProtector passwordProtector, ILogger<RegisterUserCommandHandler> logger)
        {
            _context = context;
            _validator = validator;
            _mapper = mapper;
            _mediator = mediator;
            _passwordProtector = passwordProtector;
            _logger = logger;
        }

        public async Task<RegisterUserCommandResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request is null)
                {
                    return new RegisterUserCommandResult(RegisterUserCommandResultType.BadRequest, default, new NullReferenceException("Register command can not be a null-reference object."));
                }

                ValidationContext<RegisterUserCommand> validationContext = new ValidationContext<RegisterUserCommand>(request);
                ValidationResult validationResult = await _validator.ValidateAsync(validationContext, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return new RegisterUserCommandResult(RegisterUserCommandResultType.ValidationFailure, default, new EntityValidationException("Register commad has validation errors.", ValidationResultHelpers.DistinctErrorsByProperty(validationResult)));
                }

                CreateFolderCommand command = new CreateFolderCommand("Root");
                CreateFolderCommandResult commandResult = await _mediator.Send(command, cancellationToken);

                if (commandResult.ResultType != CreateFolderCommandResultType.FolderCreated)
                {
                    return new RegisterUserCommandResult(RegisterUserCommandResultType.InternalServerError, default, commandResult.Exception);
                }

                Folder rootFolder = commandResult.GetResult();

                User user = _mapper.Map<User>(request);

                user.RootFolderID = rootFolder.ID;

                (user.Password, user.PasswordSalt) = _passwordProtector.HashPassword(request.Password);

                user.SecurityStamp = SecurityStampHelpers.GetSecurityStamp();

                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync();

                return new RegisterUserCommandResult(RegisterUserCommandResultType.UserCreated, user, default);
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogError(exception.Message);

                return new RegisterUserCommandResult(RegisterUserCommandResultType.OperationCanceled, default, exception);
            }
            catch (TimeoutException exception)
            {
                _logger.LogError(exception.Message);

                return new RegisterUserCommandResult(RegisterUserCommandResultType.OperationTimedout, default, exception);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

                return new RegisterUserCommandResult(RegisterUserCommandResultType.InternalServerError, default, exception);
            }
        }
    }

    public record RegisterUserCommandResult(RegisterUserCommandResultType ResultType, User? Result, Exception? Exception) : RequestResultTypeClassBase<RegisterUserCommandResultType, User>(ResultType, Result, Exception);

    public enum RegisterUserCommandResultType
    {
        BadRequest,
        UserCreated,
        ValidationFailure,
        OperationCanceled,
        OperationTimedout,
        InternalServerError
    }
}
