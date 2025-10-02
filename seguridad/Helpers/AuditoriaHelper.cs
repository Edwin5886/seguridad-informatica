using seguridad.Data;
using seguridad.Models;

public class AuditoriaHelper
{
    private readonly AppDbContext _context;

    public AuditoriaHelper(AppDbContext context)
    {
        _context = context;
    }

    public void Registrar(string usuario, string operacion, string tabla, Dictionary<string, (string anterior, string nuevo)> cambios)
    {
        var operacionObj = _context.Operaciones.FirstOrDefault(o => o.NombreOperacion == operacion);
        if (operacionObj == null)
            throw new Exception($"No se encontró la operación '{operacion}' en la tabla Operaciones.");

        var tablaObj = _context.TablasAuditables.FirstOrDefault(t => t.NombreTabla == tabla);
        if (tablaObj == null)
            throw new Exception($"No se encontró la tabla '{tabla}' en la tabla TablasAuditables.");

        var idOperacion = operacionObj.IdOperacion;
        var idTabla = tablaObj.IdTabla;

        var encabezado = new BitacoraEncabezado
        {
            Fecha = DateTime.Now,
            Usuario = usuario,
            IdOperacion = idOperacion,
            IdTabla = idTabla
        };
        _context.BitacoraEncabezado.Add(encabezado);
        _context.SaveChanges();

        foreach (var cambio in cambios)
        {
            _context.BitacoraDetalle.Add(new BitacoraDetalle
            {
                IdEncabezado = encabezado.IdEncabezado,
                NombreCampo = cambio.Key,
                ValorAnterior = cambio.Value.anterior,
                ValorNuevo = cambio.Value.nuevo
            });
        }
        _context.SaveChanges();
    }
}