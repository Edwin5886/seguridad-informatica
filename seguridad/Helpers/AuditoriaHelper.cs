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
        var idOperacion = _context.Operaciones.First(o => o.NombreOperacion == operacion).IdOperacion;
        var idTabla = _context.TablasAuditables.First(t => t.NombreTabla == tabla).IdTabla;

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