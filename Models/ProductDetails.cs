using System.ComponentModel.DataAnnotations;

namespace WarungElja.Models;

public class ProductDetails
{
    [Key]
    public int IdProductDetails { get; set; }

    public string? ProductName { get; set; }

    public decimal Price { get; set; }

    public int Status { get; set; }

    public virtual ICollection<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
}