using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace seguridad.Models
{
    [Table("TablasAuditables")]
    public class TablaAuditable
    {
        [Key]
        public int IdTabla { get; set; }
        [Required]
        public string NombreTabla { get; set; }
        // Puedes agregar más propiedades si lo necesitas
    }
}
