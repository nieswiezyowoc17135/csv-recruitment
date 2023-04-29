using System.ComponentModel.DataAnnotations;

namespace csv_project.Data.Models;

public class Order
{
    [Required]
    [MinLength(1), MaxLength(50)]
    public string Number { get; set; } = null!;
    [Required]
    [MinLength(1), MaxLength(50)]
    public string ClientCode { get; set; } = null!;
    [Required]
    [MinLength(1), MaxLength(50)]
    public string ClientName { get; set; } = null!;
    [Required]
    [DataType(DataType.Date)]
    public DateTime OrderDate { get; set; }
    [DataType(DataType.Date)]
    public DateTime ShipmentDate { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public bool Confirmed { get; set; }
    [Required]
    public float Value { get; set; }
}

