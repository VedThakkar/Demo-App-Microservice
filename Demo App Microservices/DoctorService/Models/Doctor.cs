using System;
using System.Collections.Generic;

namespace DoctorService.Models;

public partial class Doctor
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Department { get; set; }

    public int UserId { get; set; }
}
