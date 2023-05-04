namespace csv_project.Data.DTOs;

public class OrderDto
{
    public string Number { get; set; }
    public string ClientCode { get; set; }
    public string ClientName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ShipmentDate { get; set; }
    public int Quantity { get; set; }
    public bool Confirmed { get; set; }
    public float Value { get; set; }
}

