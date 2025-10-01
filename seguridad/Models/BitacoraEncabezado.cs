using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace seguridad.Models
{
    [Table("BitacoraEncabezado")]
    public class BitacoraEncabezado
    {
        [Key]
        public int IdEncabezado { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public int IdOperacion { get; set; }
        public int IdTabla { get; set; }
        // Puedes agregar más propiedades si lo necesitas
    }
}
