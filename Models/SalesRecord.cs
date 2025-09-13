using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarungElja.Models;

public class SalesRecord
{
    [Key]
    public int Id { get; set; }
    
    public int ProductDetailsId { get; set; }
    
    public decimal Price { get; set; }
    
    public int QuantitySold { get; set; }
    
    public DateTime SaleDate { get; set; } = DateTime.Now;
    
    [ForeignKey("ProductDetailsId")]
    public virtual ProductDetails? ProductDetails { get; set; }
}