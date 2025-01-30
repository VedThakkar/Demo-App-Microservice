using AuthService.Models;
using AuthService.Validators.IValidators;
using FluentValidation;
using FluentValidation.Results;

namespace AuthService.Validators;

public class RoleValidator : AbstractValidator<Role>, IValidatorService<Role>
{
    public RoleValidator()
    {
        // Validation for Name
        RuleFor(role => role.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        // Validation for Users collection
        RuleFor(role => role.Users)
            .NotNull().WithMessage("Users collection cannot be null.")
            .Must(users => users.Count >= 0).WithMessage("Users collection must have valid entries.");
    }

    public Task<ValidationResult> ValidateAsync(Role role)
    {
        return base.ValidateAsync(role);
    }
}