using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace seguridad.Models
{
    [Table("Operaciones")]
    public class Operacion
    {
        [Key]
        public int IdOperacion { get; set; }
        [Required]
        public string NombreOperacion { get; set; }
        // Puedes agregar más propiedades si lo necesitas
    }
}
