using DoctorService.Models;
using DoctorService.Validators.IValidators;
using FluentValidation;
using FluentValidation.Results;

namespace DoctorService.Validators;

public class DoctorValidator : AbstractValidator<Doctor>, IValidatorService<Doctor>
{
    public DoctorValidator()
    {
        RuleFor(doctor => doctor.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(3, 50).WithMessage("Name must be between 3 and 50 characters.");

        RuleFor(doctor => doctor.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive integer.")
            .When(doctor => doctor.UserId == null);

        RuleFor(doctor => doctor.Department)
            .NotNull().WithMessage("Department cannot be null.")
            .When(doctor => doctor.Department != null); // Validate only when provided
    }

    public Task<ValidationResult> ValidateAsync(Doctor doctor)
    {
        return base.ValidateAsync(doctor);
    }
}