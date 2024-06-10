namespace warehouse_management_core.DTO_s;

public class EmployeeDTO
{
    public Id Id { get; set; }
    public string Name { get; set; }
    public string PersonalId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public double Salary { get; set; }

    public static implicit operator EmployeeDTO(Employee other) =>
        new()
        {
            Id = other.Id,
            Name = other.Name,
            Email = other.Email,
            Phone = other.Phone,
            Salary = other.Salary,
        };
}
