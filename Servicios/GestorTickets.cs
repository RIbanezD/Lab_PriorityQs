using System.Globalization;
using System.Text;
using ColasDePrioridad_Ruben_Ibañez.Estructuras;
using ColasDePrioridad_Ruben_Ibañez.Modelos;

namespace ColasDePrioridad_Ruben_Ibañez.Servicios
{
    public class GestorTickets
    {
        private readonly MinHeap cola = new MinHeap();
        private readonly List<Ticket> atendidos = new List<Ticket>();

        // contadores
        public int CantidadEnCola => cola.Cantidad;
        public int CantidadAtendidos => atendidos.Count;
        public int CantidadTotal => CantidadEnCola + CantidadAtendidos;
        public bool ColaVacia => cola.EstaVacio();
        public bool HayAtendidos => atendidos.Count > 0;

        public int ContarEnColaPorPrioridad(int prioridad)
            => cola.ObtenerElementos().Count(t => t.Prioridad == prioridad);

        public int ContarAtendidosPorPrioridad(int prioridad)
            => atendidos.Count(t => t.Prioridad == prioridad);

        // Verifica correlativo
        public bool ExisteCodigo(string codigo) => Buscar(codigo) != null;

        // generacion automatica de correlativo
        public string? GenerarSiguienteCodigo()
        {
            int mayor = 0;
            foreach (Ticket t in ObtenerTodos())
                mayor = Math.Max(mayor, Ticket.ObtenerCorrelativo(t.Codigo));

            if (mayor < Ticket.CorrelativoMaximo)
                return Ticket.FormatearCodigo(mayor + 1);

            for (int n = 1; n <= Ticket.CorrelativoMaximo; n++)
            {
                string candidato = Ticket.FormatearCodigo(n);
                if (!ExisteCodigo(candidato)) return candidato;
            }
            return null;
        }

        // Registrar Ticket
        public int Registrar(Ticket ticket)
        {
            if (ExisteCodigo(ticket.Codigo)) return -1;
            return cola.Insertar(ticket);
        }

        // Agrega un ticket por medio del csv, maneja duplicados y devuelve null si ya esta registrado
        public bool Importar(Ticket ticket)
        {
            if (ExisteCodigo(ticket.Codigo)) return false;

            if (ticket.Atendido)
                atendidos.Add(ticket);
            else
                cola.Insertar(ticket);

            return true;
        }

        // Ver ticket siguiente
        public Ticket? VerSiguiente() => cola.Peek();

        // Atiende el siguiente ticket
        public Ticket? Atender()
        {
            Ticket? ticket = cola.ExtraerMin();
            if (ticket == null) return null;

            ticket.MarcarAtendido();
            atendidos.Add(ticket);
            return ticket;
        }

        // Reabrir ticket
        public int Reabrir(string codigo)
        {
            Ticket? ticket = atendidos.FirstOrDefault(t =>
                string.Equals(t.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
            if (ticket == null) return -1;

            atendidos.Remove(ticket);
            ticket.MarcarPendiente();
            return cola.Insertar(ticket);
        }

        // Ultimo ticket atendido
        public Ticket? ObtenerUltimoAtendido()
            => atendidos.OrderByDescending(t => t.FechaAtencion).FirstOrDefault();

        // Buscar ticket por codigo
        public Ticket? Buscar(string codigo)
            => cola.Buscar(codigo)
               ?? atendidos.FirstOrDefault(t => string.Equals(t.Codigo, codigo, StringComparison.OrdinalIgnoreCase));

        // Tickets pendientes
        public List<Ticket> ObtenerCola() => cola.ObtenerElementos();

        // Ticket atendidos
        public List<Ticket> ObtenerAtendidos()
            => atendidos.OrderBy(t => t.FechaAtencion).ToList();

        // Todos los tickets
        public List<Ticket> ObtenerTodos()
        {
            var todos = cola.ObtenerElementos();
            todos.AddRange(atendidos);
            return todos.OrderBy(t => t.FechaRegistro).ThenBy(t => t.Codigo, StringComparer.Ordinal).ToList();
        }

        // Busquedas por diferentes campos
        public List<Ticket> BuscarPorPrioridad(int prioridad)
            => Ordenar(ObtenerTodos().Where(t => t.Prioridad == prioridad));

        public List<Ticket> BuscarPorEstado(bool soloAtendidos)
            => Ordenar(ObtenerTodos().Where(t => t.Atendido == soloAtendidos));

        public List<Ticket> BuscarPorCliente(string texto)
            => Ordenar(FiltrarPorTexto(ObtenerTodos(), texto, t => t.Cliente));

        public List<Ticket> BuscarPorPalabraClave(string texto)
            => Ordenar(FiltrarPorTexto(ObtenerTodos(), texto, t => t.Descripcion));


        private static IEnumerable<Ticket> FiltrarPorTexto(IEnumerable<Ticket> tickets, string texto, Func<Ticket, string> campo)
        {
            string[] palabras = Normalizar(texto).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (palabras.Length == 0) return Enumerable.Empty<Ticket>();

            return tickets.Where(t =>
            {
                string contenido = Normalizar(campo(t));
                return palabras.All(p => contenido.Contains(p, StringComparison.Ordinal));
            });
        }

        private static string Normalizar(string texto)
        {
            string descompuesto = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(descompuesto.Length);
            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        private static List<Ticket> Ordenar(IEnumerable<Ticket> tickets)
        {
            var lista = tickets.ToList();
            var pendientes = lista.Where(t => !t.Atendido).OrderBy(t => t).ToList();
            var atendidosOrdenados = lista.Where(t => t.Atendido).OrderBy(t => t.FechaAtencion).ToList();
            pendientes.AddRange(atendidosOrdenados);
            return pendientes;
        }
    }
}
