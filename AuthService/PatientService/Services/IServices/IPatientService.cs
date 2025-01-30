using PatientService.Models;

namespace PatientService.Services.IServices;

public interface IPatientService
{
    Task<List<Patient>> GetAllPatientsAsync();
    Task<Patient> GetPatientByIdAsync(int id);
    Task<Patient> AddPatientAsync(Patient patient);
    Task<Patient> UpdatePatientAsync(int id, Patient updatedPatient);
    Task<Patient> DeletePatientAsync(int id);

    Task<Patient> GetByUserIdAsync(int userId);

}