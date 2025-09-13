using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarungElja.Models;

public class ProductStock
{
    [Key]
    public int IdWarungElja { get; set; }

    public int IdProductDetails { get; set; }

    public int Stock { get; set; }

    [ForeignKey("IdProductDetails")]
    public virtual ProductDetails? ProductDetails { get; set; }
}