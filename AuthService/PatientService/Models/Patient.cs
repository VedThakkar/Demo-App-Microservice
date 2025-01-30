using System;
using System.Collections.Generic;

namespace PatientService.Models;

public partial class Patient
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }
}
