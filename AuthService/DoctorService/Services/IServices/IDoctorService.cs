using DoctorService.Models;

namespace DoctorService.Services.IServices;

public interface IDoctorService
{
    Task<List<Doctor>> GetAllDoctorsAsync();
    Task<Doctor> GetDoctorByIdAsync(int id);
    Task<Doctor> AddDoctorAsync(Doctor doctor);
    Task<Doctor> UpdateDoctorAsync(int id, Doctor updatedDoctor);
    Task<Doctor> DeleteDoctorAsync(int id);

    Task<Doctor> GetByUserIdAsync(int userId);

}