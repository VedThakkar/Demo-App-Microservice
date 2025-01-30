
using AuthService.Messages;
using DoctorService.Models;
using DoctorService.Repositories.IRepositories;
using DoctorService.Services.IServices;
using DoctorService.Validators.IValidators;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace DoctorService.Services;

public class DoctorService : IDoctorService
{
    private readonly IGenericRepository<Doctor> _doctorRepository;
    private readonly IValidatorService<Doctor> _doctorValidator;
    private readonly IBus _bus;

    public DoctorService(IGenericRepository<Doctor> doctorRepository, IValidatorService<Doctor> doctorValidator, IBus bus)
    {
        _doctorRepository = doctorRepository;
        _doctorValidator = doctorValidator;
        _bus = bus;
    }
    
    public async Task<List<Doctor>> GetAllDoctorsAsync()
    {
        var doctors = await _doctorRepository.GetAllAsync();
        return doctors.ToList();
    }

    public async Task<Doctor> GetDoctorByIdAsync(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);

        if (doctor == null)
        {
            return null;
        }

        return await _doctorRepository.GetByIdAsync(id);
    }

    public async Task<Doctor> AddDoctorAsync(Doctor doctor)
    {
        ValidationResult validationResult = await _doctorValidator.ValidateAsync(doctor);

        if (!validationResult.IsValid)
        {
            // If validation fails, create a collection of ValidationFailure objects
            var validationFailures = validationResult.Errors.Select(e => new ValidationFailure(e.PropertyName, e.ErrorMessage)).ToList();

            // Throw the ValidationException with the collection of ValidationFailure objects
            throw new ValidationException(validationFailures);
        }
        return await _doctorRepository.AddAsync(doctor);
    }

    public async Task<Doctor> UpdateDoctorAsync(int id, Doctor updatedDoctor)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null) return null;
        
        ValidationResult validationResult = await _doctorValidator.ValidateAsync(updatedDoctor);

        if (!validationResult.IsValid)
        {
            // If validation fails, create a collection of ValidationFailure objects
            var validationFailures = validationResult.Errors.Select(e => new ValidationFailure(e.PropertyName, e.ErrorMessage)).ToList();

            // Throw the ValidationException with the collection of ValidationFailure objects
            throw new ValidationException(validationFailures);
        }

        updatedDoctor.Id = doctor.Id;
        updatedDoctor.UserId = doctor.UserId;
        return await _doctorRepository.UpdateAsync(updatedDoctor);
    }

    public async Task<Doctor> DeleteDoctorAsync(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null) return null;


        await _bus.Publish(new DoctorDeleted
        {
            UserId = doctor.UserId,
            DoctorId = doctor.Id,
            Name = doctor.Name
        });
        //await _doctorRepository.DeleteAsync(id);
        return doctor;
    }
    
    public async Task<Doctor> GetByUserIdAsync(int userId)
    { 
        // Get all patients as IQueryable to support LINQ methods like FirstOrDefaultAsync
        var doctors = await _doctorRepository.GetAllAsync();
        var doctor = doctors.FirstOrDefault(d => d.UserId == userId);

        return doctor;

    }
    
}