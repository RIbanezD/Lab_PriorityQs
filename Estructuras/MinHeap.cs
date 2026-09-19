using ColasDePrioridad_Ruben_Ibañez.Modelos;

namespace ColasDePrioridad_Ruben_Ibañez.Estructuras
{
    public class MinHeap
    {
        private const int CapacidadInicial = 16;

        private Ticket[] elementos;
        private int cantidad;

        public MinHeap()
        {
            elementos = new Ticket[CapacidadInicial];
            cantidad = 0;
        }

        // Número de tickets almacenados actualmente en el heap
        public int Cantidad => cantidad;

        public bool EstaVacio() => cantidad == 0;

        // Posicion en el array
        private static int Padre(int i) => (i - 1) / 2;
        private static int HijoIzquierdo(int i) => 2 * i + 1;
        private static int HijoDerecho(int i) => 2 * i + 2;

        // Insertar ticket
        public int Insertar(Ticket ticket)
        {
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));

            // Suma 16 espacios extra si se llena
            if (cantidad == elementos.Length)
                MaxCapacidad();

            // Se ubica al final del arbol
            int posicion = cantidad;
            elementos[posicion] = ticket;
            cantidad++;

            // Autobalanceo
            return HeapUp(posicion);
        }

        // Heap-up
        private int HeapUp(int indice)
        {
            while (indice > 0)
            {
                int padre = Padre(indice);

                if (elementos[indice].CompareTo(elementos[padre]) >= 0)
                    break;

                Intercambiar(indice, padre);
                indice = padre;
            }
            return indice;
        }

        // ver Ticket mas urgente y antiguo
        public Ticket? Peek() => cantidad == 0 ? null : elementos[0];

        // eliminar raiz y auto balancear
        public Ticket? ExtraerMin()
        {
            if (cantidad == 0) return null;

            Ticket minimo = elementos[0];

            // El último nodo ocupa la posición de la raíz.
            cantidad--;
            elementos[0] = elementos[cantidad];
            elementos[cantidad] = null!;

            // Se reorganiza hacia abajo si aún quedan elementos.
            if (cantidad > 0)
                HeapDown(0);

            return minimo;
        }

        // Heap-Down
        private void HeapDown(int indice)
        {
            while (true)
            {
                int izquierdo = HijoIzquierdo(indice);
                int derecho = HijoDerecho(indice);
                int menor = indice;

                if (izquierdo < cantidad && elementos[izquierdo].CompareTo(elementos[menor]) < 0)
                    menor = izquierdo;

                if (derecho < cantidad && elementos[derecho].CompareTo(elementos[menor]) < 0)
                    menor = derecho;

                // Si el nodo ya es menor que sus hijos, la propiedad del heap se cumple.
                if (menor == indice)
                    break;

                Intercambiar(indice, menor);
                indice = menor;
            }
        }

        // Buscar Ticket por codigo
        public Ticket? Buscar(string codigo)
        {
            for (int i = 0; i < cantidad; i++)
            {
                if (string.Equals(elementos[i].Codigo, codigo, StringComparison.OrdinalIgnoreCase))
                    return elementos[i];
            }
            return null;
        }

        // ver tickets en orden
        public List<Ticket> ObtenerElementos()
        {
            var lista = new List<Ticket>(cantidad);
            for (int i = 0; i < cantidad; i++)
                lista.Add(elementos[i]);
            return lista;
        }

        // Intercambio de valores en el arbol
        private void Intercambiar(int a, int b)
        {
            Ticket temporal = elementos[a];
            elementos[a] = elementos[b];
            elementos[b] = temporal;
        }

        // 
        private void MaxCapacidad()
        {
            var nuevo = new Ticket[elementos.Length + 16];
            for (int i = 0; i < cantidad; i++)
                nuevo[i] = elementos[i];
            elementos = nuevo;
        }
    }
}
