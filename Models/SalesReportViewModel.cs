using System.ComponentModel.DataAnnotations;

namespace WarungElja.Models;

public class SalesReportViewModel
{
    public int Id { get; set; }
    
    public string? ProductName { get; set; }
    
    public decimal Price { get; set; }
    
    public int QuantitySold { get; set; }
    
    public DateTime SaleDate { get; set; }
    
    public decimal TotalAmount => Price * QuantitySold;
}