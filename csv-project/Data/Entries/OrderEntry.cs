using System.ComponentModel.DataAnnotations;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

namespace csv_project.Data.Entries;

public class OrderEntry
{
    [Required, MinLength(1), MaxLength(50)]
    [Name("Number")]
    public string Number { get; set; } = null!;

    [Name("ClientCode")]
    public string ClientCode { get; set; } = null!;

    [Name("ClientName")]
    public string ClientName { get; set; } = null!;

    [Name("OrderDate")]
    public DateTime OrderDate { get; set; }
    [Name("ShipmentDate")]
    public DateTime? ShipmentDate { get; set; }
    [Name("Quantity")]
    public int Quantity { get; set; }
    [Name("Confirmed")]
    public bool Confirmed { get; set; }
    [Name("Value")]
    public float Value { get; set; }
}

internal sealed class OrderEntryMap : ClassMap<OrderEntry>
{
    public OrderEntryMap()
    {
        Map(m => m.Number);
        Map(m => m.ClientCode);
        Map(m => m.ClientName);
        Map(m => m.OrderDate).TypeConverter<CustomOrderDateTimeConverter>();
        Map(m => m.ShipmentDate).TypeConverter<CustomShipmentDateTimeConverter>();
        Map(m => m.Quantity).TypeConverter<CustomQuantityConverter>();
        Map(m => m.Confirmed).TypeConverter<CustomConfirmedConverter>();
        Map(m => m.Value).TypeConverter<CustomValueConverter>();
    }
}

internal class CustomValueConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text == null) throw new ArgumentException("Invalid data.");
        var x = float.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
        return x;
    }
}

internal class CustomConfirmedConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text is "1") return true;
        if (text is "0") return false;
        throw new ArgumentException("Invalid data.");
    }
}

internal class CustomQuantityConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text == null) throw new ArgumentException("Invalid data.");
        return Int32.Parse(text);
    }
}

internal class CustomOrderDateTimeConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (DateTime.TryParse(text, out var dateTime))
        {
            return dateTime;
        }
        throw new ArgumentException("Invalid data.");
    }
}

internal class CustomShipmentDateTimeConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null!;
        }
        if (DateTime.TryParse(text, out var dateTime))
        {
            return dateTime;
        }
        throw new ArgumentException("Invalid data.");
    }
}

