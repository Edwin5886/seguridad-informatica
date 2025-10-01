using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace seguridad.Models
{
    [Table("BitacoraDetalle")]
    public class BitacoraDetalle
    {
        [Key]
        public int IdDetalle { get; set; }

        [ForeignKey("BitacoraEncabezado")]
        public int IdEncabezado { get; set; }

        [Required]
        public string NombreCampo { get; set; }

        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }

        // Relación de navegación (opcional)
        public BitacoraEncabezado BitacoraEncabezado { get; set; }
    }
}
