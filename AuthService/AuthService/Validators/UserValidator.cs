using AuthService.Models;
using AuthService.Validators.IValidators;
using FluentValidation;
using FluentValidation.Results;

namespace AuthService.Validators;

public class UserValidator : AbstractValidator<User>, IValidatorService<User>
{
    public UserValidator()
    {
        RuleFor(user => user.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.");

        RuleFor(user => user.PasswordHash)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(user => user.RoleId)
            .Must(roleId => new[] { 1, 2, 3 }.Contains(roleId))
            .WithMessage("RoleId must be either 1, 2, or 3.");

        RuleFor(user => user.CreatedAt)
            .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("CreatedAt cannot be in the future.")
            .When(user => user.CreatedAt.HasValue); // Only validate if CreatedAt is provided

        // Skip validation for Refreshtokens during registration
        RuleFor(user => user.Refreshtokens)
            .NotNull().WithMessage("RefreshTokens collection cannot be null.")
            .Must(tokens => tokens != null && tokens.Count > 0).WithMessage("At least one refresh token is required.")
            .When(user => user.Refreshtokens != null && user.Refreshtokens.Any())
            .WithMessage("RefreshTokens collection cannot be empty.")
            .Unless(user => user.Refreshtokens == null); // Skip validation if Refreshtokens are not provided
    }

    public Task<ValidationResult> ValidateAsync(User user)
    {
        return base.ValidateAsync(user);
    }
}