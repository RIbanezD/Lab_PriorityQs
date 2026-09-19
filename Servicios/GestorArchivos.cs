using System.Globalization;
using System.Text;
using ColasDePrioridad_Ruben_Ibañez.Modelos;

namespace ColasDePrioridad_Ruben_Ibañez.Servicios
{
    public static class GestorArchivos
    {
        private const string NombreArchivo = "tickets.csv";
        private const string Encabezado = "Codigo,Cliente,Descripcion,Prioridad,Estado,FechaRegistro,FechaAtencion";
        private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss.fff";

        // Ruta archivo
        public static string RutaArchivo => Path.Combine(AppContext.BaseDirectory, NombreArchivo);

        public static bool ExisteArchivo() => File.Exists(RutaArchivo);

        // Guardar datos
        public static string Guardar(IEnumerable<Ticket> tickets)
        {
            var lineas = new List<string> { Encabezado };

            foreach (Ticket t in tickets.OrderBy(x => x.FechaRegistro).ThenBy(x => x.Codigo, StringComparer.Ordinal))
            {
                lineas.Add(string.Join(",",
                    CaracteresEspeciales(t.Codigo),
                    CaracteresEspeciales(t.Cliente),
                    CaracteresEspeciales(t.Descripcion),
                    t.Prioridad.ToString(CultureInfo.InvariantCulture),
                    t.Estado,
                    t.FechaRegistro.ToString(FormatoFecha, CultureInfo.InvariantCulture),
                    t.FechaAtencion.HasValue
                        ? t.FechaAtencion.Value.ToString(FormatoFecha, CultureInfo.InvariantCulture)
                        : string.Empty));
            }

            File.WriteAllLines(RutaArchivo, lineas, new UTF8Encoding(true));
            return RutaArchivo;
        }

        // Importar datos
        public static List<Ticket> Importar(out List<string> errores)
        {
            errores = new List<string>();
            var tickets = new List<Ticket>();

            string[] lineas = File.ReadAllLines(RutaArchivo, Encoding.UTF8);

            for (int i = 0; i < lineas.Length; i++)
            {
                int numeroLinea = i + 1;
                string linea = lineas[i];

                if (string.IsNullOrWhiteSpace(linea)) continue;

                if (i == 0 && linea.TrimStart('\uFEFF').StartsWith("Codigo", StringComparison.OrdinalIgnoreCase))
                    continue;

                List<string> campos = DividirLineaCsv(linea);
                if (campos.Count < 5)
                {
                    errores.Add($"Línea {numeroLinea}: se esperaban al menos 5 columnas.");
                    continue;
                }

                if (!Ticket.TryNormalizarCodigo(campos[0], out string codigo))
                {
                    errores.Add($"Línea {numeroLinea}: código inválido '{campos[0]}'.");
                    continue;
                }

                string cliente = campos[1].Trim();
                string descripcion = campos[2].Trim();
                if (cliente.Length == 0 || descripcion.Length == 0)
                {
                    errores.Add($"Línea {numeroLinea}: el cliente y la descripción no pueden estar vacíos.");
                    continue;
                }

                if (!int.TryParse(campos[3].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int prioridad)
                    || prioridad < 1 || prioridad > 5)
                {
                    errores.Add($"Línea {numeroLinea}: la prioridad debe ser un número entre 1 y 5.");
                    continue;
                }

                string estado = campos[4].Trim();
                bool atendido;
                if (estado.Equals("Atendido", StringComparison.OrdinalIgnoreCase)) atendido = true;
                else if (estado.Equals("Pendiente", StringComparison.OrdinalIgnoreCase)) atendido = false;
                else
                {
                    errores.Add($"Línea {numeroLinea}: estado inválido '{estado}' (use Pendiente o Atendido).");
                    continue;
                }

                DateTime fechaRegistro = campos.Count > 5 && TryLeerFecha(campos[5], out DateTime fr)
                    ? fr
                    : DateTime.Now;

                DateTime? fechaAtencion = null;
                if (atendido)
                    fechaAtencion = campos.Count > 6 && TryLeerFecha(campos[6], out DateTime fa) ? fa : DateTime.Now;

                tickets.Add(new Ticket(codigo, cliente, descripcion, prioridad, fechaRegistro, atendido, fechaAtencion));
            }

            return tickets;
        }


        private static bool TryLeerFecha(string texto, out DateTime fecha)
            => DateTime.TryParse(texto.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);

        // utiliza comillas con caracteres especiales
        private static string CaracteresEspeciales(string valor)
        {
            valor = valor.Replace("\r", " ").Replace("\n", " ");
            if (valor.Contains(',') || valor.Contains('"'))
                return "\"" + valor.Replace("\"", "\"\"") + "\"";
            return valor;
        }

        private static List<string> DividirLineaCsv(string linea)
        {
            var campos = new List<string>();
            var actual = new StringBuilder();
            bool entreComillas = false;

            for (int i = 0; i < linea.Length; i++)
            {
                char c = linea[i];

                if (entreComillas)
                {
                    if (c == '"')
                    {
                        if (i + 1 < linea.Length && linea[i + 1] == '"')
                        {
                            actual.Append('"');
                            i++;
                        }
                        else
                        {
                            entreComillas = false;
                        }
                    }
                    else
                    {
                        actual.Append(c);
                    }
                }
                else if (c == '"')
                {
                    entreComillas = true;
                }
                else if (c == ',')
                {
                    campos.Add(actual.ToString());
                    actual.Clear();
                }
                else
                {
                    actual.Append(c);
                }
            }

            campos.Add(actual.ToString());
            return campos;
        }
    }
}
