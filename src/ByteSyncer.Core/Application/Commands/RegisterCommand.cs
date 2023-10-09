using AutoMapper;
using ByteSyncer.Core.Files.Commands;
using ByteSyncer.Core.Helpers;
using ByteSyncer.Data.EF;
using ByteSyncer.Domain.Application.Models;
using ByteSyncer.Domain.Contracts;
using ByteSyncer.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ByteSyncer.Core.Application.Commands
{
    public record RegisterCommand(string? Email, string? GivenName, string? FamilyName, string? Password, string? PasswordRepeat) : IRequest<RegisterCommandResult>;

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterCommandResult>
    {
        private readonly DataContext _context;
        private readonly IValidator<RegisterCommand> _validator;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPasswordProtector _passwordProtector;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(DataContext context, IValidator<RegisterCommand> validator, IMapper mapper, IMediator mediator, IPasswordProtector passwordProtector, ILogger<RegisterCommandHandler> logger)
        {
            _context = context;
            _validator = validator;
            _mapper = mapper;
            _mediator = mediator;
            _passwordProtector = passwordProtector;
            _logger = logger;
        }

        public async Task<RegisterCommandResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request is null)
                {
                    return new RegisterCommandResult(RegisterCommandResultType.InternalServerError, default, new NullReferenceException("Register command can not be a null-reference object."));
                }

                ValidationContext<RegisterCommand> validationContext = new ValidationContext<RegisterCommand>(request);
                ValidationResult validationResult = await _validator.ValidateAsync(validationContext, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return new RegisterCommandResult(RegisterCommandResultType.ValidationFailure, default, new EntityValidationException("Register commad has validation errors.", ValidationResultHelpers.DistinctErrorsByProperty(validationResult)));
                }

                CreateFolderCommand createFolderCommand = new CreateFolderCommand("Root");
                CreateFolderCommandResult createFolderCommandResult = await _mediator.Send(createFolderCommand, cancellationToken);

                if (createFolderCommandResult.Result != CreateFolderCommandResultType.Succeded)
                {
                    return new RegisterCommandResult(RegisterCommandResultType.InternalServerError, default, createFolderCommandResult.Exception);
                }

                User userToCreate = _mapper.Map<User>(request);

                userToCreate.RootFolderID = createFolderCommandResult.Folder.ID;

                (userToCreate.Password, userToCreate.PasswordSalt) = _passwordProtector.HashPassword(request.Password);

                userToCreate.SecurityStamp = SecurityStampHelpers.GetSecurityStamp();

                await _context.Users.AddAsync(userToCreate, cancellationToken);
                await _context.SaveChangesAsync();

                return new RegisterCommandResult(RegisterCommandResultType.Succeded, userToCreate, default);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

                return new RegisterCommandResult(RegisterCommandResultType.InternalServerError, default, exception);
            }
        }
    }

    public record RegisterCommandResult(RegisterCommandResultType Result, User? User, Exception? Exception);

    public enum RegisterCommandResultType
    {
        Succeded,
        ValidationFailure,
        InternalServerError
    }
}
