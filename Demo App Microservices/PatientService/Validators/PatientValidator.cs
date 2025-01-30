
using FluentValidation;
using FluentValidation.Results;
using PatientService.Models;
using PatientService.Validators.IValidators;

namespace PatientService.Validators;

public class PatientValidator : AbstractValidator<Patient>, IValidatorService<Patient>
{
    public PatientValidator()
    {
        RuleFor(patient => patient.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(3, 50).WithMessage("Name must be between 3 and 50 characters.");

        RuleFor(patient => patient.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive integer.")
            .When(patient => patient.UserId == null);

        RuleFor(patient => patient.DateOfBirth)
            .Must(date => date <= DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("Date of birth cannot be in the future.")
            .When(patient => patient.DateOfBirth.HasValue); // Validate only when provided

        RuleFor(patient => patient.Gender)
            .Must(gender => new[] { "Male", "Female", "Other" }.Contains(gender))
            .WithMessage("Gender must be either 'Male', 'Female', or 'Other'.")
            .When(patient => !string.IsNullOrEmpty(patient.Gender)); // Validate only when provided
        

    }

    public Task<ValidationResult> ValidateAsync(Patient patient)
    {
        return base.ValidateAsync(patient);
    }
}
