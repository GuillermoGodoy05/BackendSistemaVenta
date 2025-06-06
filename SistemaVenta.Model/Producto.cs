using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Model;

public partial class Producto
{
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")] 
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public int? IdCategoria { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio.")] 
    [Range(0.01, 99999999.99, ErrorMessage = "El precio debe ser mayor a cero.")] 
    [Column(TypeName = "decimal(10,2)")] 
    public decimal? Precio { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 999999, ErrorMessage = "El stock no puede ser negativo.")] 
    public int? Stock { get; set; }

    [Required(ErrorMessage = "El estado 'Es Activo' es obligatorio.")] 
    public bool? EsActivo { get; set; }

    public DateTime? FechaRegistro { get; set; } 

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual Categoria? IdCategoriaNavigation { get; set; }
}
