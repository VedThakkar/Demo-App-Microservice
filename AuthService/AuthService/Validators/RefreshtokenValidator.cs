using AuthService.Models;
using AuthService.Validators.IValidators;
using FluentValidation;
using FluentValidation.Results;

namespace AuthService.Validators;

public class RefreshtokenValidator : AbstractValidator<Refreshtoken>, IValidatorService<Refreshtoken>
{
    public RefreshtokenValidator()
    {
        var now = DateTime.UtcNow;
        
        // Validation for Token
        RuleFor(token => token.Token)
            .NotEmpty().WithMessage("Token is required.")
            .MaximumLength(255).WithMessage("Token cannot exceed 255 characters.");

        // Validation for ExpiresAt
        RuleFor(token => token.ExpiresAt)
            .GreaterThan(_ => DateTime.UtcNow).WithMessage("ExpiresAt must be a future date.");

        // Validation for CreatedAt
        RuleFor(token => token.CreatedAt)
            .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("CreatedAt cannot be in the future.");

        // Validation for UserId
        RuleFor(token => token.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive integer.");

        // Conditional validation for User
        RuleFor(token => token.User)
            .NotNull().WithMessage("User cannot be null.")
            .When(token => token.User != null); // Validate only when provided
    }

    public Task<ValidationResult> ValidateAsync(Refreshtoken refreshtoken)
    {
        return base.ValidateAsync(refreshtoken);
    }
}