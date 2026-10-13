using System.Globalization;

namespace PaginaWeb.Helpers
{
    // Formatos de la tienda para Argentina: $ 1.800,50 y fechas dd/MM/yyyy
    public static class Formato
    {
        private static readonly CultureInfo Argentina = CultureInfo.GetCultureInfo("es-AR");

        public static string Precio(decimal valor) => valor.ToString("C", Argentina);

        public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy", Argentina);

        public static string FechaHora(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm", Argentina);
    }
}
