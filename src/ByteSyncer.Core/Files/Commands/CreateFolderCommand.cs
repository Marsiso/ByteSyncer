using AutoMapper;
using ByteSyncer.Data.EF;
using ByteSyncer.Domain.Files.Models;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ByteSyncer.Core.Files.Commands
{
    public record CreateFolderCommand(string? Name) : IRequest<CreateFolderCommandResult>;

    public class CreateFolderCommandHandler : IRequestHandler<CreateFolderCommand, CreateFolderCommandResult>
    {
        private readonly DataContext _context;
        private readonly IValidator<CreateFolderCommand> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateFolderCommandHandler> _logger;

        public CreateFolderCommandHandler(DataContext context, IValidator<CreateFolderCommand> validator, IMapper mapper, ILogger<CreateFolderCommandHandler> logger)
        {
            _context = context;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CreateFolderCommandResult> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request is null)
                {
                    return new CreateFolderCommandResult(CreateFolderCommandResultType.InternalServerError, default, new NullReferenceException("Create folder command can not be a null-reference object."));
                }

                ValidationContext<CreateFolderCommand> validationContext = new ValidationContext<CreateFolderCommand>(request);
                ValidationResult validationResult = await _validator.ValidateAsync(validationContext, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return new CreateFolderCommandResult(CreateFolderCommandResultType.ValidationFailure, default, new ValidationException("Create folder commad is invalid.", validationResult.Errors));
                }

                Folder folder = _mapper.Map<Folder>(request);

                await _context.Folders.AddAsync(folder, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return new CreateFolderCommandResult(CreateFolderCommandResultType.Succeded, folder, default);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

                return new CreateFolderCommandResult(CreateFolderCommandResultType.InternalServerError, default, exception);
            }
        }
    }

    public record CreateFolderCommandResult(CreateFolderCommandResultType Result, Folder? Folder, Exception? Exception);

    public enum CreateFolderCommandResultType
    {
        Succeded,
        ValidationFailure,
        InternalServerError
    }
}
