using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;


namespace SistemaVenta.BLL.Servicios
{
    public class VentaService : IVentaService
    {

        private readonly IVentaRepository _ventaRepositorio;
        private readonly IGenericRepository<DetalleVenta> _detalleVentaRepositorio;   
        private readonly IMapper _mapper;

        public VentaService(
            IVentaRepository ventaRepositorio, 
            IGenericRepository<DetalleVenta> detalleVentaRepositorio,
            IMapper mapper
        )
        {
            _ventaRepositorio = ventaRepositorio;
            _detalleVentaRepositorio = detalleVentaRepositorio;
            _mapper = mapper;
        }

        
        public async Task<VentaDTO> Registrar(VentaDTO modelo)
        {
            try
            {
                var ventaGenerada = await _ventaRepositorio.Registrar(_mapper.Map<Venta>(modelo));

                if (ventaGenerada.IdVenta == 0) throw new TaskCanceledException("No se pudo crear");
                                               

                return _mapper.Map<VentaDTO>(ventaGenerada);
            }
            catch (InvalidOperationException ex) // Captura excepciones específicas de la lógica de negocio o de la DAL.
            {
                // Comentario: Esto captura el error si IdVenta es 0 o si no se encontró un recurso (DAL).
                throw new Exception($"Error en la lógica de negocio al registrar la venta: {ex.Message}", ex);
            }
            catch (ArgumentException ex) // Para validaciones de entrada de la BLL
            {
                // Comentario: Si se añaden validaciones aquí, esta captura las desviaciones de argumentos.
                throw new Exception($"Datos de venta inválidos: {ex.Message}", ex);
            }
            catch (Exception ex) // Captura cualquier otra excepción, incluyendo las de la DAL.
            {
                // Comentario: Este catch final encapsula cualquier otro error inesperado al registrar la venta.
                throw new Exception("Error al registrar la venta.", ex);
            }

        }

        public async Task<List<VentaDTO>> Historial(string buscarPor, string numeroVenta, string fechaInicio, string fechaFin)
        {
            IQueryable<Venta> query = await _ventaRepositorio.Consultar();
            var ListaResultado = new List<Venta>();

            try
            {
                if (buscarPor == "fecha")
                {
                    DateTime fecha_Inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("en-US"));
                    DateTime fecha_Fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("en-US"));

                    ListaResultado = await query.Where(v =>
                       v.FechaRegistro.Value.Date >= fecha_Inicio.Date &&
                       v.FechaRegistro.Value.Date <= fecha_Fin.Date
                    ).Include(dv => dv.DetalleVenta)
                    .ThenInclude(p => p.IdProductoNavigation)
                    .ToListAsync();
                }
                else
                {
                    ListaResultado = await query.Where(v => v.NumeroDocumento == numeroVenta)
                        .Include(dv => dv.DetalleVenta)
                        .ThenInclude(p => p.IdProductoNavigation)
                        .ToListAsync();
                }
            }
            catch (FormatException ex) 
            {
                // Se lanza si las fechas no tienen el formato esperado.
                throw new ArgumentException("El formato de las fechas proporcionadas es incorrecto. Asegúrese de usar 'dd/MM/yyyy'.", ex);
            }
            catch (ArgumentException ex) // Captura las excepciones de validación que añadimos.
            {
                // Captura errores de validación de argumentos específicos.
                throw new Exception($"Error de argumento al buscar historial: {ex.Message}", ex);
            }
            catch (Exception ex) // Captura cualquier otra excepción, incluyendo las de la DAL.
            {
                // Este catch final encapsula cualquier otro error inesperado al obtener el historial.
                throw new Exception("No se pudo obtener el historial de ventas.", ex);
            }

            return _mapper.Map<List<VentaDTO>>(ListaResultado);

        }


        public async Task<List<ReporteDTO>> Reporte(string fechaInicio, string fechaFin)
        {
            IQueryable<DetalleVenta> query = await _detalleVentaRepositorio.Consultar();
            var ListaResultado = new List<DetalleVenta>();

            try
            {
                DateTime fecha_Inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("en-US"));
                DateTime fecha_Fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("en-US"));

                ListaResultado = await query
                    .Include(p => p.IdProductoNavigation)
                    .Include(p => p.IdVentaNavigation)
                    .Where(dv => 
                        dv.IdVentaNavigation.FechaRegistro.Value.Date >= fecha_Inicio.Date && 
                        dv.IdVentaNavigation.FechaRegistro.Value.Date <= fecha_Fin.Date                         
                    )
                    .ToListAsync();         
            }
            catch (FormatException ex) 
            {
                // Se lanza si las fechas del reporte no tienen el formato esperado.
                throw new ArgumentException("El formato de las fechas para el reporte es incorrecto. Asegúrese de usar 'dd/MM/yyyy'.", ex);
            }
            catch (ArgumentException ex) // Captura las excepciones de validación que añadimos.
            {
                // Captura errores de validación de argumentos específicos del reporte.
                throw new Exception($"Error de argumento al generar reporte: {ex.Message}", ex);
            }
            catch (Exception ex) // Captura cualquier otra excepción, incluyendo las de la DAL.
            {
                // Este catch final encapsula cualquier otro error inesperado al generar el reporte.
                throw new Exception("No se pudo obtener el reporte de ventas.", ex);
            }

            return _mapper.Map<List<ReporteDTO>>(ListaResultado);

        }
    }
}
