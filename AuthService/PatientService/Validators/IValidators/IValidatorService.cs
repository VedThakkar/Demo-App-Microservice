namespace PatientService.Validators.IValidators;

public interface IValidatorService<T> where T : class
{
    Task<FluentValidation.Results.ValidationResult> ValidateAsync(T entity);
}