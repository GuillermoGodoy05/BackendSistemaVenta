using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Model;

public partial class Venta
{
    public int IdVenta { get; set; }

    // El número de documento se genera en la DAL, por lo que no es Required aquí para el input inicial.
    // [Required(ErrorMessage = "El número de documento es obligatorio.")] // Podría ser requerido si se genera antes de este punto
    //[StringLength(20, ErrorMessage = "El número de documento no puede exceder los 20 caracteres.")] 
    public string? NumeroDocumento { get; set; }

    [Required(ErrorMessage = "El tipo de pago es obligatorio.")] 
    public string? TipoPago { get; set; }

    
    [Range(0.01, 999999999.99, ErrorMessage = "El total debe ser mayor a cero.")] 
    [Column(TypeName = "decimal(18,2)")] // Asegura el tipo de dato decimal con precisión en la base de datos.
    public decimal? Total { get; set; }

    // La fecha de registro se suele generar automáticamente por la aplicación o la base de datos.
    // [Required(ErrorMessage = "La fecha de registro es obligatoria.")] // Podría ser requerido si se espera del input
    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();
}
