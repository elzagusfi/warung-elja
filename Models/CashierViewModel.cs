using System.ComponentModel.DataAnnotations;

namespace WarungElja.Models;

public class CashierViewModel
{
    public int IdProductDetails { get; set; }
    
    public string? ProductName { get; set; }
    
    public int CurrentStock { get; set; }
    
    [Display(Name = "Quantity to Sell")]
    public int SellQuantity { get; set; }
    
    public decimal Price { get; set; }
}