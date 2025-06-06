using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.Model;


namespace SistemaVenta.DAL.Repositorios
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        private readonly DbventaContext _dbcontext;

        public VentaRepository(DbventaContext dbcontext) : base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Venta> Registrar(Venta modelo)
        {
            Venta ventaGenerada = new Venta();

            using (var transaction = _dbcontext.Database.BeginTransaction())
            { 
                try
                {
                    foreach (DetalleVenta itemDv in modelo.DetalleVenta) { 
                        Producto producto_encontrado = _dbcontext.Productos.Where(p => p.IdProducto == itemDv.IdProducto).First();

                        producto_encontrado.Stock = producto_encontrado.Stock - itemDv.Cantidad;
                        _dbcontext.Productos.Update(producto_encontrado);
                    
                    }

                    await _dbcontext.SaveChangesAsync();


                    // Código para transacción que se crea de forma temporal

                    NumeroDocumento correlativo = _dbcontext.NumeroDocumentos.First();

                    correlativo.UltimoNumero = correlativo.UltimoNumero + 1; 
                    correlativo.FechaRegistro = DateTime.Now;

                    _dbcontext.NumeroDocumentos.Update(correlativo);
                    await _dbcontext.SaveChangesAsync();

                    // Generar el formato del numero de documento de venta
                    int CantidadDigitos = 4;
                    string ceros = string.Concat(Enumerable.Repeat("0", CantidadDigitos));
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();

                    // Corta desde x (x , cuantos dígitos va a obtener)
                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - CantidadDigitos, CantidadDigitos);

                    modelo.NumeroDocumento = numeroVenta;

                    await _dbcontext.Venta.AddAsync(modelo);
                    await _dbcontext.SaveChangesAsync();

                    ventaGenerada = modelo;

                    // Si toda la operación que hemos registrado es correcta. Finaliza la transacción

                    transaction.Commit();

                }
                catch (DbUpdateException ex) // Captura errores relacionados con la base de datos (Ej. stock negativo, FK, unicidad)
                {
                    transaction.Rollback();
                    // Comentario: Lanza una excepción específica si hay un conflicto de base de datos durante el registro.
                    throw new Exception("Error al registrar la venta debido a un conflicto de datos (ej. stock insuficiente, referencia inválida).", ex);
                }
                catch (InvalidOperationException ex) // Captura errores como .First() si no encuentra nada
                {
                    transaction.Rollback();
                    // Comentario: Lanza una excepción si no se encontró un producto o correlativo requerido.
                    throw new Exception("Error al registrar la venta: Recurso no encontrado (ej. producto inexistente, correlativo no configurado).", ex);
                }
                catch (Exception ex) // Captura cualquier otra excepción inesperada
                {
                    transaction.Rollback();
                    // Comentario: Lanza una excepción genérica para cualquier otro error inesperado durante el registro de la venta.
                    throw new Exception("Error inesperado al registrar la venta.", ex);
                }

                return ventaGenerada;
            }

        }
    }
}
