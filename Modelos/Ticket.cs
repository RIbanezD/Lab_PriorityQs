using System.Globalization;

namespace ColasDePrioridad_Ruben_Ibañez.Modelos
{
    // Getters/Setters - Ticket con IComparable para delimitar cual es menor
    public class Ticket : IComparable<Ticket>
    {
        public const string Prefijo = "TCK";

        public const int DigitosCorrelativo = 4;

        public const int CorrelativoMaximo = 9999;

        public string Codigo { get; }
        public string Cliente { get; }
        public string Descripcion { get; }

        public int Prioridad { get; }

        public DateTime FechaRegistro { get; }

        public bool Atendido { get; private set; }

        // Fecha y hora de atención; null mientras el ticket esta en pendiente
        public DateTime? FechaAtencion { get; private set; }

        public string Estado => Atendido ? "Atendido" : "Pendiente";

        public string NombrePrioridad => NombrePrioridadDe(Prioridad);

        public string PrioridadTexto => $"{Prioridad} ({NombrePrioridad})";

        // ticket nuevo en estado Pendiente, registrado en este momento
        public Ticket(string codigo, string cliente, string descripcion, int prioridad)
            : this(codigo, cliente, descripcion, prioridad, DateTime.Now, false, null)
        {
        }

        // Crea un ticket con todos sus datos (se usa al importar desde CSV)
        public Ticket(string codigo, string cliente, string descripcion, int prioridad,
                      DateTime fechaRegistro, bool atendido, DateTime? fechaAtencion)
        {
            if (prioridad < 1 || prioridad > 5)
                throw new ArgumentOutOfRangeException(nameof(prioridad), "La prioridad debe estar entre 1 y 5.");

            Codigo = codigo;
            Cliente = cliente;
            Descripcion = descripcion;
            Prioridad = prioridad;
            FechaRegistro = fechaRegistro;
            Atendido = atendido;
            FechaAtencion = atendido ? (fechaAtencion ?? DateTime.Now) : null;
        }

        public void MarcarAtendido()
        {
            Atendido = true;
            FechaAtencion = DateTime.Now;
        }

        public void MarcarPendiente()
        {
            Atendido = false;
            FechaAtencion = null;
        }

        // Ordenamiento por criticalidad, orden de llegada y correlativo
        public int CompareTo(Ticket? otro)
        {
            if (otro is null) return -1;

            int resultado = Prioridad.CompareTo(otro.Prioridad);
            if (resultado != 0) return resultado;

            resultado = FechaRegistro.CompareTo(otro.FechaRegistro);
            if (resultado != 0) return resultado;

            return string.CompareOrdinal(Codigo, otro.Codigo);
        }

        public static string FormatearCodigo(int correlativo)
            => Prefijo + correlativo.ToString("D" + DigitosCorrelativo, CultureInfo.InvariantCulture);

        /// Valida y normaliza un código escrito por el usuario. Mayusculas, minusculas, "TCK5" o simplemente "5", y siempre devuelve el formato oficial "TCK0005".
        public static bool TryNormalizarCodigo(string? entrada, out string codigo)
        {
            codigo = string.Empty;
            if (string.IsNullOrWhiteSpace(entrada)) return false;

            string texto = entrada.Trim().ToUpperInvariant();
            if (texto.StartsWith(Prefijo, StringComparison.Ordinal))
                texto = texto.Substring(Prefijo.Length);

            if (texto.Length < 1 || texto.Length > DigitosCorrelativo) return false;

            // Validaacion digitos 0-9
            foreach (char c in texto)
                if (c < '0' || c > '9') return false;

            int correlativo = int.Parse(texto, CultureInfo.InvariantCulture);
            if (correlativo < 1) return false;

            codigo = FormatearCodigo(correlativo);
            return true;
        }

        // numero correlativo
        public static int ObtenerCorrelativo(string codigo)
            => TryNormalizarCodigo(codigo, out string normalizado)
                ? int.Parse(normalizado.Substring(Prefijo.Length), CultureInfo.InvariantCulture)
                : 0;

        public static string NombrePrioridadDe(int prioridad) => prioridad switch
        {
            1 => "Crítica",
            2 => "Alta",
            3 => "Media",
            4 => "Baja",
            5 => "Muy Baja",
            _ => "Desconocida"
        };

        public static string DescripcionPrioridadDe(int prioridad) => prioridad switch
        {
            1 => "El servicio principal se encuentra fuera de operación. Requiere atención inmediata.",
            2 => "El sistema presenta fallos importantes que afectan el trabajo de múltiples usuarios.",
            3 => "Existe un error funcional que afecta parcialmente las operaciones.",
            4 => "Solicitudes de configuración, ajustes o consultas generales.",
            5 => "Solicitudes de mejora, cambios menores o restablecimiento de contraseña.",
            _ => string.Empty
        };
    }
}
