using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                } catch{
                    transaction.Rollback();
                    throw;
                }

                return ventaGenerada;
            }





        }
    }
}
