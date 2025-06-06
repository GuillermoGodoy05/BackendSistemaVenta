using System;
using System.Collections.Generic;
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
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _usuarioRepositorio;
        private readonly IMapper _mapper;

        public UsuarioService(IGenericRepository<Usuario> usuarioRepositorio, IMapper mapper)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _mapper = mapper;
        }

        // Implementación CRUD


        public async Task<List<UsuarioDTO>> Lista()
        {
            try
            {
                var queryUsuario = await _usuarioRepositorio.Consultar();
                var listaUsuarios = queryUsuario.Include(rol => rol.IdRolNavigation).ToList();  
                return _mapper.Map<List<UsuarioDTO>>(listaUsuarios);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }

        }
        public async Task<SesionDTO> ValidarCredenciales(string correo, string clave)
        {

            try
            {
                var queryUsuario = await _usuarioRepositorio.Consultar( u =>
                    u.Correo == correo
                );

                var usuario = await queryUsuario.Include(u => u.IdRolNavigation).FirstOrDefaultAsync();

                if (usuario == null)
                    throw new TaskCanceledException("El usuario no existe");

                // Verificar la clave con BCrypt
                bool claveValida = BCrypt.Net.BCrypt.Verify(clave, usuario.Clave);

                if (!claveValida)
                    throw new TaskCanceledException("Contraseña incorrecta");

                if (queryUsuario.FirstOrDefault() == null) throw new TaskCanceledException("El usuario no existe");    

                // Se añade el rol al usuario - First porque seguro hay info 
                Usuario usuarioCompleto = queryUsuario.Include(rol => rol.IdRolNavigation).First();
                                
                return _mapper.Map<SesionDTO>(usuarioCompleto);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }


        }
        public async Task<UsuarioDTO> Crear(UsuarioCrearDTO modelo)
        {
            try
            {
                var existe = await _usuarioRepositorio.Obtener(u => u.Correo == modelo.Correo);

                if (existe != null)
                    throw new TaskCanceledException("Ya existe un usuario con ese correo");
                
                modelo.Clave = BCrypt.Net.BCrypt.HashPassword(modelo.Clave);

                var usuarioCreado = await _usuarioRepositorio.Crear(_mapper.Map<Usuario>(modelo));

                if (usuarioCreado.IdUsuario == 0) throw new TaskCanceledException("No se puedo crear");
                

                var query = await _usuarioRepositorio.Consultar(u => u.IdUsuario == usuarioCreado.IdUsuario);

                usuarioCreado = query.Include(rol => rol.IdRolNavigation).First();

                return _mapper.Map<UsuarioDTO>(usuarioCreado);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }


        }

        public async Task<bool> Editar(UsuarioDTO modelo)
        {
            try
            {
                var usuarioModelo = _mapper.Map<Usuario>(modelo);

                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == usuarioModelo.IdUsuario);

                if (usuarioEncontrado == null) throw new TaskCanceledException("El usuario no existe");

                usuarioEncontrado.NombreCompleto = usuarioModelo.NombreCompleto;
                usuarioEncontrado.Correo = usuarioModelo.Correo;
                usuarioEncontrado.IdRol = usuarioModelo.IdRol;

                if (!string.IsNullOrEmpty(usuarioModelo.Clave))
                {
                    usuarioEncontrado.Clave = BCrypt.Net.BCrypt.HashPassword(usuarioModelo.Clave);
                }

                usuarioEncontrado.EsActivo = usuarioModelo.EsActivo;

                bool respuesta = await _usuarioRepositorio.Editar(usuarioEncontrado);

                if (!respuesta) throw new TaskCanceledException("No se pudo editar");

                return respuesta;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }

        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == id);

                if (usuarioEncontrado == null) throw new TaskCanceledException("El usuario no existe");

                bool respuesta = await _usuarioRepositorio.Eliminar(usuarioEncontrado);

                if (!respuesta) throw new TaskCanceledException("No se pudo eliminar");

                return respuesta;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }

        }



    }
}
