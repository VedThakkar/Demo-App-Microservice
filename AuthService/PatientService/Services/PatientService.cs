using AuthService.Messages;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PatientService.Models;
using PatientService.Repositories.IRepositories;
using PatientService.Services.IServices;
using PatientService.Validators.IValidators;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace PatientService.Services;

public class PatientService : IPatientService
{
    private readonly IGenericRepository<Patient> _patientRepository;
    private readonly IValidatorService<Patient> _patientValidator;
    private readonly IBus _bus;

    public PatientService(IGenericRepository<Patient> patientRepository, IValidatorService<Patient> patientValidator, IBus bus)
    {
        _patientRepository = patientRepository;
        _patientValidator = patientValidator;
        _bus = bus;
    }

    public async Task<List<Patient>> GetAllPatientsAsync()
    {
        var patients = await _patientRepository.GetAllAsync();
        return patients.ToList();
    }

    public async Task<Patient> GetPatientByIdAsync(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);

        if (patient == null) return null;
        
        return await _patientRepository.GetByIdAsync(id);
    }

    public async Task<Patient> AddPatientAsync(Patient patient)
    {
        ValidationResult validationResult = await _patientValidator.ValidateAsync(patient);

        if (!validationResult.IsValid)
        {
            // If validation fails, create a collection of ValidationFailure objects
            var validationFailures = validationResult.Errors.Select(e => new ValidationFailure(e.PropertyName, e.ErrorMessage)).ToList();

            // Throw the ValidationException with the collection of ValidationFailure objects
            throw new ValidationException(validationFailures);
        }
        return await _patientRepository.AddAsync(patient);
    }

    public async Task<Patient> UpdatePatientAsync(int id, Patient updatedPatient)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null) return null;
        
        ValidationResult validationResult = await _patientValidator.ValidateAsync(updatedPatient);

        if (!validationResult.IsValid)
        {
            // If validation fails, create a collection of ValidationFailure objects
            var validationFailures = validationResult.Errors.Select(e => new ValidationFailure(e.PropertyName, e.ErrorMessage)).ToList();

            // Throw the ValidationException with the collection of ValidationFailure objects
            throw new ValidationException(validationFailures);
        }

        updatedPatient.Id = id; // Ensure the ID is maintained
        updatedPatient.UserId = patient.UserId;
        return await _patientRepository.UpdateAsync(updatedPatient);
    }

        public async Task<Patient> DeletePatientAsync(int id)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null) return null;

            //await _patientRepository.DeleteAsync(id);
            await _bus.Publish(new PatientDeleted
            {
                UserId = patient.UserId,
                PatientId = patient.Id,
                Name = patient.Name
            });
            
            return patient;
        }
        
        public async Task<Patient> GetByUserIdAsync(int userId)
        { 
            // Get all patients as IQueryable to support LINQ methods like FirstOrDefaultAsync
            var patients = await _patientRepository.GetAllAsync();
            var patient = patients.FirstOrDefault(p => p.UserId == userId);

            return patient;

        }
}
