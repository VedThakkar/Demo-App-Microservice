using System.Security.Claims;
using DoctorService.AuthProvider.IAuthProvider;
using DoctorService.Models;
using DoctorService.Services.IServices;
using FluentValidation;
using Serilog;

namespace DoctorService.APIs;

public static class DoctorEndpoints
{
    public static IEndpointRouteBuilder MapDoctorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/doctors", GetAllDoctors).RequireAuthorization();
        endpoints.MapGet("/doctors/{id}", GetDoctorById).RequireAuthorization();
        endpoints.MapPost("/doctors", AddDoctor).RequireAuthorization("AdminOnly");
        endpoints.MapPut("/doctors/{id}", UpdateDoctor).RequireAuthorization("AdminDoctor");
        endpoints.MapDelete("/doctors/{id}", DeleteDoctor).RequireAuthorization("AdminDoctor");

        return endpoints;
    }

     private static async Task<IResult> GetAllDoctors(IDoctorService doctorService)
    {
        try
        {
            Log.Information("Retrieving all doctors.");
            var doctors = await doctorService.GetAllDoctorsAsync();
            return Results.Ok(doctors);
        }
        catch (ValidationException ex)
        {
            Log.Warning("Validation failed for Getting All Doctors.");
            return Results.BadRequest(new { Message = "Invalid doctor data", Errors = ex.Errors });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving all doctors");
            return Results.Problem("An error occurred while fetching doctors.");
        }
    }

    private static async Task<IResult> GetDoctorById(int id, IDoctorService doctorService)
    {
        try
        {
            Log.Information("Retrieving doctor with ID {Id}", id);
            var doctor = await doctorService.GetDoctorByIdAsync(id);
            if (doctor == null)
            {
                Log.Warning("Doctor with ID {Id} not found", id);
                return Results.NotFound(new { Message = "Doctor not found." });
            }
            
            /*var userClaims = claimsProvider.GetClaims();
            // Check if userClaims is null or empty
            if (userClaims == null)
            {
                Log.Error("User claims are null.");
                return Results.Problem("User claims are null.");
            }
            if (userClaims.IsInRole("doctor") && !userClaims.HasClaim(ClaimTypes.NameIdentifier, doctor.UserId.ToString()))
                return Results.Forbid();*/

            return Results.Ok(doctor);
        }
        catch (ValidationException ex)
        {
            Log.Warning("Validation failed for Getting Doctor By Id.");
            return Results.BadRequest(new { Message = "Invalid doctor data", Errors = ex.Errors });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving doctor with ID {Id}", id);
            return Results.Problem("An error occurred while fetching the doctor.");
        }
    }
    
    private static async Task<IResult> AddDoctor(Doctor doctor, IDoctorService doctorService)
    {
        try
        {
            if (doctor == null)
            {
                return Results.BadRequest(new { Message = "Invalid Doctor data." });
            }

            Log.Information("Creating a new doctor");
            var addedDoctor = await doctorService.AddDoctorAsync(doctor);
            return Results.Created($"/doctors/{addedDoctor.Id}", addedDoctor);
        }
        catch (ValidationException ex)
        {
            Log.Warning("Validation failed for Adding Doctor.");
            return Results.BadRequest(new { Message = "Invalid doctor data", Errors = ex.Errors });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while creating a new doctor");
            return Results.Problem("An error occurred while adding the doctor.");
        }
    }

    private static async Task<IResult> UpdateDoctor(int id, Doctor updatedDoctor, IDoctorService doctorService, ClaimsPrincipal user)
    {
        try
        {
            var doctor1 = await doctorService.GetDoctorByIdAsync(id);
            if (doctor1 == null)
            {
                Log.Warning("Doctor with ID {Id} not found", id);
                return Results.NotFound(new { Message = "Doctor not found." });
            }
            if (user.IsInRole("doctor") && !user.HasClaim(ClaimTypes.NameIdentifier, doctor1.UserId.ToString()))
                return Results.Forbid();
            /*var userClaims = claimsProvider.GetClaims();
            if (userClaims.IsInRole("doctor") && !userClaims.HasClaim(ClaimTypes.NameIdentifier, doctor1.UserId.ToString()))
                return Results.Forbid();*/
            
            if (updatedDoctor == null)
            {
                return Results.BadRequest(new { Message = "Invalid Doctor data." });
            }

            Log.Information("Updating doctor with ID {Id}", id);
            var doctor = await doctorService.UpdateDoctorAsync(id, updatedDoctor);
            if (doctor == null)
            {
                Log.Warning("Doctor with ID {Id} not found", id);
                return Results.NotFound(new { Message = "Doctor not found." });
            }

            return Results.Ok(doctor);
        }
        catch (ValidationException ex)
        {
            Log.Warning("Validation failed for Updating Doctor.");
            return Results.BadRequest(new { Message = "Invalid doctor data", Errors = ex.Errors });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while updating doctor with ID {Id}", id);
            return Results.Problem("An error occurred while updating the doctor.");
        }
    }

    private static async Task<IResult> DeleteDoctor(int id, IDoctorService doctorService, ClaimsPrincipal user)
    {
        try
        {
            var doctor1 = await doctorService.GetDoctorByIdAsync(id);
            
            if (user.IsInRole("doctor") && !user.HasClaim(ClaimTypes.NameIdentifier, doctor1.UserId.ToString()))
                return Results.Forbid();
            /*var userClaims = claimsProvider.GetClaims();
            if (userClaims.IsInRole("doctor") && !userClaims.HasClaim(ClaimTypes.NameIdentifier, doctor1.UserId.ToString()))
                return Results.Forbid();*/

            Log.Information("Deleting doctor with ID {Id}", id);
            var doctor = await doctorService.DeleteDoctorAsync(id);
            if (doctor == null)
            {
                Log.Warning("Doctor with ID {Id} not found", id);
                return Results.NotFound(new { Message = "Doctor not found." });
            }

            return Results.NoContent();
        }
        catch (ValidationException ex)
        {
            Log.Warning("Validation failed for Deleting Doctor.");
            return Results.BadRequest(new { Message = "Invalid doctor data", Errors = ex.Errors });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deleting doctor with ID {Id}", id);
            return Results.Problem("An error occurred while deleting the doctor.");
        }
    }
    
}