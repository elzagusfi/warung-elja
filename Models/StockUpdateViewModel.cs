using System.ComponentModel.DataAnnotations;

namespace WarungElja.Models;

public class StockUpdateViewModel
{
    public int IdProductDetails { get; set; }
    
    public string? ProductName { get; set; }
    
    public int CurrentStock { get; set; }
    
    [Display(Name = "Add Stock")]
    public int AddStock { get; set; }
}