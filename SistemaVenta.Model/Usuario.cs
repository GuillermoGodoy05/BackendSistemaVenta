using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Model;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")] 
    [StringLength(100, ErrorMessage = "El nombre completo no puede exceder los 100 caracteres.")] 
    public string NombreCompleto { get; set; } = null!;

    [Required(ErrorMessage = "El correo es obligatorio.")] 
    //[EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")] 
    [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres.")]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "El rol es obligatorio.")] 
    public int? IdRol { get; set; }

    [Required(ErrorMessage = "La clave es obligatoria.")] 
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe tener entre 6 y 100.")] 
    public string Clave { get; set; } = null!;

    [Required(ErrorMessage = "El estado 'Es Activo' es obligatorio.")] 
    public bool? EsActivo { get; set; }

    public DateTime? FechaRegistro { get; set; } 

    public virtual Rol? IdRolNavigation { get; set; }


}
