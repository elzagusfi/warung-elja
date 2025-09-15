using System.ComponentModel.DataAnnotations;

namespace WarungElja.Models;

public class ProductDetails
{
    [Key]
    public int IdProductDetails { get; set; }

    public string? ProductName { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    public int Status { get; set; }

    public virtual ICollection<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
}