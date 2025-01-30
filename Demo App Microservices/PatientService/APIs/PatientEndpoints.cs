using System.Security.Claims;
using FluentValidation;
using PatientService.AuthProvider.IAuthProvider;
using PatientService.Models;
using PatientService.Services.IServices;
using Serilog;

namespace PatientService.APIs;

public static class PatientEndpoints
{
    public static IEndpointRouteBuilder MapPatientEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/patients", GetAllPatients).RequireAuthorization("AdminOnly");
        endpoints.MapGet("/patients/{id}", GetPatientById).RequireAuthorization("AdminPatient");
        endpoints.MapPost("/patients", AddPatient).RequireAuthorization("AdminOnly");
        endpoints.MapPut("/patients/{id}", UpdatePatient).RequireAuthorization("AdminPatient");
        endpoints.MapDelete("/patients/{id}", DeletePatient).RequireAuthorization("AdminPatient");

        return endpoints;
    }

    private static async Task<IResult> GetAllPatients(IPatientService patientService)
    {
        try
        {
            Log.Information("Retrieving all Patients");
            var patients = await patientService.GetAllPatientsAsync();
            return Results.Ok(patients);
        }
        catch (ValidationException ex)
        {
            Log.Warning("Validation failed for GetAllPatients.");
            return Results.BadRequest(new { Message = "Invalid patients data", Errors = ex.Errors });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving all patients");
            return Results.Problem("An error occurred while fetching patients.");
        }
    }

    private static async Task<IResult> GetPatientById(int id, IPatientService patientService, ClaimsPrincipal user)
    {
        try
        {
            Log.Information("Retrieving patient with ID {Id}", id);
            var patient = await patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                Log.Warning("Patient with ID {Id} not found", id);
                
                return Results.NotFound(new { Message = "Patient not found." });
            }
            if (user.IsInRole("user") && !user.HasClaim(ClaimTypes.NameIdentifier, patient.UserId.ToString()))
                return Results.Forbid();
            //var userClaims = claimsProvider.GetClaims();
            // Check if userClaims is null or empty
            /*if (userClaims == null)
            {
                Log.Error("User claims are null.");
                return Results.Problem("User claims are null.");
            }
            // Log claims to inspect the content
            //Log.Information("User Claims: {Claims}", string.Join(", ", userClaims.Claims.Select(c => $"{c.Type}: {c.Value}")));

            if (userClaims.IsInRole("patient") && !userClaims.HasClaim(ClaimTypes.NameIdentifier, patient.UserId.ToString()))
                return Results.Forbid();*/
            
            //await rabitMqProducer.SendMessage(patient, "patient_queue");
            return Results.Ok(patient);
        }
        catch (ValidationException ex)
        {
            Log.Warning("Validation failed for GetAllPatientById.");
            return Results.BadRequest(new { Message = "Invalid patients data", Errors = ex.Errors });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving patient with ID {Id}", id);
            return Results.Problem("An error occurred while fetching the patient.");
        }
    }

    private static async Task<IResult> AddPatient(Patient patient, IPatientService patientService)
    {
        try
        {
            if (patient == null)
            {
                return Results.BadRequest(new { Message = "Invalid patient data." });
            }

            Log.Information("Creating a new patient");
            var addedPatient = await patientService.AddPatientAsync(patient);
            if (addedPatient == null)
            {
                return Results.BadRequest();
            }
            return Results.Created($"/patients/{addedPatient.Id}", addedPatient);
        }
        catch (ValidationException ex)
        {
            Log.Error(ex, "An Validation error occurred while creating a new patient");
            // Return BadRequest with validation errors
            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToList();
            return Results.BadRequest(new { Errors = errors });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while creating a new patient");
            return Results.Problem("An error occurred while adding the patient.");
        }
    }

    private static async Task<IResult> UpdatePatient(int id, Patient updatedPatient, IPatientService patientService, ClaimsPrincipal user)
    {
        try
        {
            var patient1 = await patientService.GetPatientByIdAsync(id);
            if (patient1 == null)
            {
                Log.Warning("Patient with ID {Id} not found", id);
                return Results.NotFound(new { Message = "Patient not found." });
            }
            if (user.IsInRole("user") && !user.HasClaim(ClaimTypes.NameIdentifier, patient1.UserId.ToString()))
                return Results.Forbid();
            /*var userClaims = claimsProvider.GetClaims();
            if (userClaims.IsInRole("patient") && !userClaims.HasClaim(ClaimTypes.NameIdentifier, patient1.UserId.ToString()))
                return Results.Forbid();*/
            
            if (updatedPatient == null)
            {
                return Results.BadRequest(new { Message = "Invalid patient data." });
            }

            Log.Information("Updating patient with ID {Id}", id);
            var patient = await patientService.UpdatePatientAsync(id, updatedPatient);
            if (patient == null)
            {
                Log.Warning("Patient with ID {Id} not found", id);
                return Results.NotFound(new { Message = "Patient not found." });
            }

            return Results.Ok(patient);
        }
        catch (ValidationException ex)
        {
            // Handle validation exceptions, returning validation errors as part of the BadRequest response
            Log.Warning("Validation failed for Updating Patient.");
            return Results.BadRequest(new { Message = "Invalid patient data", Errors = ex.Errors });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while updating patient with ID {Id}", id);
            return Results.Problem("An error occurred while updating the patient.");
        }
    }

    private static async Task<IResult> DeletePatient(int id, IPatientService patientService, ClaimsPrincipal user)
    {
        try
        {
            var patient1 = await patientService.GetPatientByIdAsync(id);
            if (user.IsInRole("user") && !user.HasClaim(ClaimTypes.NameIdentifier, patient1.UserId.ToString()))
                return Results.Forbid(); 
            /*var userClaims = claimsProvider.GetClaims();
            if (userClaims.IsInRole("patient") && !userClaims.HasClaim(ClaimTypes.NameIdentifier, patient1.UserId.ToString()))
                return Results.Forbid();*/

            Log.Information("Deleting patient with ID {Id}", id);
            var patient = await patientService.DeletePatientAsync(id);
            if (patient == null)
            {
                Log.Warning("Patient with ID {Id} not found", id);
                return Results.NotFound(new { Message = "Patient not found." });
            }

            return Results.NoContent();
        }
        catch (ValidationException ex)
        {
            // Handle validation exceptions, returning validation errors as part of the BadRequest response
            Log.Warning("Validation failed for Deleting Patient.");
            return Results.BadRequest(new { Message = "Invalid patient data", Errors = ex.Errors });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deleting patient with ID {Id}", id);
            return Results.Problem("An error occurred while deleting the patient.");
        }
    }
}