using System.Globalization;
using ColasDePrioridad_Ruben_Ibañez.Modelos;
using ColasDePrioridad_Ruben_Ibañez.Servicios;
using ColasDePrioridad_Ruben_Ibañez.Utilidades;
using Spectre.Console;

namespace ColasDePrioridad_Ruben_Ibañez
{
    public class Program
    {
        private static readonly GestorTickets gestor = new GestorTickets();

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Bienvenida();
            Import();

            bool salir = false;
            while (!salir)
            {
                MostrarEncabezado();
                MostrarMenuPrincipal();

                string opcion = Validaciones.LeerTexto("Seleccione una opción:", permitirVacio: true);

                switch (opcion)
                {
                    case "1": RegistrarTicket(); break;
                    case "2": MostrarSiguienteTicket(); break;
                    case "3": AtenderTicket(); break;
                    case "4": MostrarColaDeTickets(); break;
                    case "5": MenuBuscarTicket(); break;
                    case "6": MostrarCantidadTickets(); break;
                    case "7": ReabrirTicket(); break;
                    case "8": GuardarDatos(); break;
                    case "9": ImportarDatos(); break;
                    case "10":
                        salir = true;
                        break;
                    default:
                        AnsiConsole.MarkupLine("[red]Opción inválida. Presione una tecla para continuar...[/]");
                        Console.ReadKey(true);
                        break;
                }
            }

            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Este peluche se va a su estuche").Centered().Color(Color.SkyBlue1));
            AnsiConsole.MarkupLine("[grey]La máquina de chambear lo hizo otra vez.[/]");
        }

        // Menu
        private static void Bienvenida()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("TechSolutions")
                    .Centered()
                    .Color(Color.Navy));
            AnsiConsole.MarkupLine("[bold white]Sistema de Gestión de Tickets de Soporte[/]");
            AnsiConsole.WriteLine();
            Validaciones.ReturnMenu();
        }

        private static void MostrarEncabezado()
        {
            AnsiConsole.Clear();
            var regla = new Rule("[bold cyan]SISTEMA DE GESTIÓN DE TICKETS DE SOPORTE[/]")
            {
                Justification = Justify.Center,
                Style = Style.Parse("cyan")
            };
            AnsiConsole.Write(regla);
            AnsiConsole.WriteLine();
        }

        private static void MostrarMenuPrincipal()
        {
            var panel = new Panel(
                "[green][[1]][/] Registrar Ticket\n" +
                "[green][[2]][/] Mostrar Siguiente Ticket\n" +
                "[green][[3]][/] Atender Ticket\n" +
                "[green][[4]][/] Mostrar Cola de Tickets\n" +
                "[green][[5]][/] Buscar Ticket\n" +
                "[green][[6]][/] Mostrar Cantidad de Tickets\n" +
                "[green][[7]][/] Reabrir Ticket Atendido\n" +
                "[green][[8]][/] Guardar Datos\n" +
                "[green][[9]][/] Importar Datos\n" +
                "[green][[10]][/] Salir")
            {
                Header = new PanelHeader(" Menú Principal "),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("cyan")
            };
            AnsiConsole.Write(panel);

            // Resumen rápido del estado actual del sistema.
            AnsiConsole.MarkupLine(
                $"[grey]En cola:[/] [bold cyan]{gestor.CantidadEnCola}[/]   [grey]|[/]   " +
                $"[grey]Atendidos:[/] [bold green]{gestor.CantidadAtendidos}[/]");
            AnsiConsole.WriteLine();
        }

        // Registro de Ticket
        private static void RegistrarTicket()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Registro de Ticket[/]").LeftJustified());
            AnsiConsole.WriteLine();

            string? codigo = CorrelativoTicket();
            if (codigo == null)
            {
                AnsiConsole.MarkupLine("[grey]Registro cancelado.[/]");
                Validaciones.ReturnMenu();
                return;
            }

            AnsiConsole.MarkupLine($"[grey]Código asignado:[/] [bold]{codigo}[/]");
            AnsiConsole.WriteLine();

            string cliente = Validaciones.LeerTexto("Nombre del Cliente:");
            string descripcion = Validaciones.LeerTexto("Descripción del Problema:");

            AnsiConsole.WriteLine();
            MostrarLeyendaPrioridades();
            int prioridad = Validaciones.LeerEntero("Nivel de Prioridad (1-5):", 1, 5);

            var ticket = new Ticket(codigo, cliente, descripcion, prioridad);

            int posicion = gestor.Registrar(ticket);

            AnsiConsole.WriteLine();
            if (posicion < 0)
            {
                AnsiConsole.MarkupLine($"[red]Ya existe un ticket con el código {codigo}. No se permiten duplicados.[/]");
            }
            else
            {
                MostrarDetalleTicket(ticket, "Registro de Ticket");
                AnsiConsole.MarkupLine("[bold green]Ticket registrado exitosamente.[/]");
            }

            Validaciones.ReturnMenu();
        }

        // Correlativo manual o automatico
        private static string? CorrelativoTicket()
        {
            string? sugerido = gestor.GenerarSiguienteCodigo();

            string opcionAutomatica = sugerido != null
                ? $"[green][[1]][/] Correlativo automático (siguiente: [bold]{sugerido}[/])\n"
                : "[grey][[1]] Correlativo automático (no disponible: se agotaron los códigos)[/]\n";

            var panel = new Panel(
                opcionAutomatica +
                "[green][[2]][/] Código manual (TCK + 4 dígitos, ejemplo: TCK0025)\n" +
                "[green][[3]][/] Cancelar registro")
            {
                Header = new PanelHeader(" Código del Ticket "),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("cyan")
            };
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();

            while (true)
            {
                string opcion = Validaciones.LeerTexto("¿Cómo desea asignar el código?", permitirVacio: true);

                switch (opcion)
                {
                    case "1":
                        if (sugerido != null) return sugerido;
                        AnsiConsole.MarkupLine("[red]El correlativo automático no está disponible. Use el código manual.[/]");
                        break;

                    case "2":
                        return LeerCodigoManual();

                    case "3":
                        return null;

                    default:
                        AnsiConsole.MarkupLine("[red]Opción inválida. Elija 1, 2 o 3.[/]");
                        break;
                }
            }
        }

        private static string? LeerCodigoManual()
        {
            AnsiConsole.MarkupLine("[grey]Formato: TCK + 4 dígitos (ejemplo: TCK0025). También puede escribir solo los dígitos.[/]");

            while (true)
            {
                string? codigo = Validaciones.LeerCodigoTicket(
                    "Código del Ticket (deje vacío para cancelar):", permitirCancelar: true);

                if (codigo == null) return null;

                if (gestor.ExisteCodigo(codigo))
                {
                    AnsiConsole.MarkupLine($"[red]Ya existe un ticket con el código {codigo}. No se permiten duplicados.[/]");
                    continue;
                }

                return codigo;
            }
        }

        // Ver siguiente ticket
        private static void MostrarSiguienteTicket()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Siguiente Ticket a Atender[/]").LeftJustified());
            AnsiConsole.WriteLine();

            Ticket? siguiente = gestor.VerSiguiente();

            if (siguiente == null)
            {
                MostrarMensajeColaVacia();
            }
            else
            {
                MostrarDetalleTicket(siguiente, "Resultado de la Consulta");
            }

            Validaciones.ReturnMenu();
        }

        // Atender ticket
        private static void AtenderTicket()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Atender Ticket[/]").LeftJustified());
            AnsiConsole.WriteLine();

            Ticket? siguiente = gestor.VerSiguiente();

            if (siguiente == null)
            {
                MostrarMensajeColaVacia();
                Validaciones.ReturnMenu();
                return;
            }

            // Confirmacion antes de atender
            MostrarDetalleTicket(siguiente, "Ticket a Atender");
            AnsiConsole.WriteLine();

            if (!Validaciones.Confirmar($"¿Confirma que desea atender el ticket {siguiente.Codigo}?"))
            {
                AnsiConsole.MarkupLine("[grey]Operación cancelada. El ticket permanece en la cola.[/]");
                Validaciones.ReturnMenu();
                return;
            }

            Ticket? atendido = gestor.Atender();

            AnsiConsole.WriteLine();
            if (atendido != null)
            {
                MostrarDetalleTicket(atendido, "Resultado de la Operación");
                AnsiConsole.MarkupLine("[bold green]Ticket atendido correctamente.[/]");
                AnsiConsole.MarkupLine($"[grey]Tickets pendientes restantes: {gestor.CantidadEnCola}[/]");
            }

            Validaciones.ReturnMenu();
        }

        // Mostar priority-q
        private static void MostrarColaDeTickets()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Cola de Prioridad Actual[/]").LeftJustified());
            AnsiConsole.WriteLine();

            List<Ticket> cola = gestor.ObtenerCola();

            if (cola.Count == 0)
            {
                MostrarMensajeColaVacia();
                Validaciones.ReturnMenu();
                return;
            }

            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
            tabla.AddColumn("Posición");
            tabla.AddColumn("Ticket");
            tabla.AddColumn("Cliente");
            tabla.AddColumn("Descripción");
            tabla.AddColumn("Prioridad");

            // Se muestran en el orden del arreglo
            for (int i = 0; i < cola.Count; i++)
            {
                Ticket t = cola[i];
                string c = EstiloPrioridad.MarkupDe(t.Prioridad);

                tabla.AddRow(
                    $"[{c}]{i}{(i == 0 ? "" : "")}[/]",
                    $"[bold {c}]{Markup.Escape(t.Codigo)}[/]",
                    $"[{c}]{Markup.Escape(t.Cliente)}[/]",
                    $"[{c}]{Markup.Escape(t.Descripcion)}[/]",
                    $"[bold {c}]{Markup.Escape(t.PrioridadTexto)}[/]");
            }

            AnsiConsole.Write(tabla);
            AnsiConsole.MarkupLine($"[bold]Total de Tickets: {cola.Count}[/]");
            AnsiConsole.WriteLine();

            Validaciones.ReturnMenu();
        }

        // buscar ticket
        private static void MenuBuscarTicket()
        {
            bool volver = false;
            while (!volver)
            {
                MostrarEncabezado();
                var panel = new Panel(
                    "[green][[1]][/] Por Código (correlativo)\n" +
                    "[green][[2]][/] Por Prioridad\n" +
                    "[green][[3]][/] Por Estado (pendientes o atendidos)\n" +
                    "[green][[4]][/] Por Cliente\n" +
                    "[green][[5]][/] Por Palabra Clave (en la descripción)\n" +
                    "[green][[6]][/] Volver al Menú Principal")
                {
                    Header = new PanelHeader(" Búsqueda de Ticket "),
                    Border = BoxBorder.Rounded,
                    BorderStyle = Style.Parse("cyan")
                };
                AnsiConsole.Write(panel);
                AnsiConsole.WriteLine();

                string opcion = Validaciones.LeerTexto("Seleccione un criterio de búsqueda:", permitirVacio: true);

                switch (opcion)
                {
                    case "1": BuscarPorCodigo(); break;
                    case "2": BuscarPorPrioridad(); break;
                    case "3": BuscarPorEstado(); break;
                    case "4": BuscarPorCliente(); break;
                    case "5": BuscarPorPalabraClave(); break;
                    case "6": volver = true; break;
                    default:
                        AnsiConsole.MarkupLine("[red]Opción inválida.[/]");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

        private static void BuscarPorCodigo()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Búsqueda por Código[/]").LeftJustified());
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Formato: TCK + 4 dígitos. También puede escribir solo los dígitos.[/]");

            string codigo = Validaciones.LeerCodigoTicket("Ingrese el código del ticket:")!;
            Ticket? resultado = gestor.Buscar(codigo);

            AnsiConsole.WriteLine();
            if (resultado != null)
                MostrarDetalleTicket(resultado, "Resultado de la Búsqueda");
            else
                AnsiConsole.MarkupLine($"[red]No se encontró ningún ticket con el código {codigo}.[/]");

            Validaciones.ReturnMenu();
        }

        private static void BuscarPorPrioridad()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Búsqueda por Prioridad[/]").LeftJustified());
            AnsiConsole.WriteLine();

            MostrarLeyendaPrioridades();
            int prioridad = Validaciones.LeerEntero("Nivel de Prioridad a buscar (1-5):", 1, 5);

            List<Ticket> resultados = gestor.BuscarPorPrioridad(prioridad);

            AnsiConsole.WriteLine();
            MostrarTablaTickets(resultados, $"Tickets con Prioridad {prioridad} ({Ticket.NombrePrioridadDe(prioridad)})");
            Validaciones.ReturnMenu();
        }

        private static void BuscarPorEstado()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Búsqueda por Estado[/]").LeftJustified());
            AnsiConsole.WriteLine();

            var panel = new Panel(
                "[green][[1]][/] Pendientes (En cola)\n" +
                "[green][[2]][/] Atendidos")
            {
                Header = new PanelHeader(" Estado del Ticket "),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("cyan")
            };
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();

            bool? soloAtendidos = null;
            while (soloAtendidos == null)
            {
                string opcion = Validaciones.LeerTexto("Seleccione el estado a consultar:");
                if (opcion == "1") soloAtendidos = false;
                else if (opcion == "2") soloAtendidos = true;
                else AnsiConsole.MarkupLine("[red]Opción inválida. Elija 1 o 2.[/]");
            }

            List<Ticket> resultados = gestor.BuscarPorEstado(soloAtendidos.Value);

            AnsiConsole.WriteLine();
            MostrarTablaTickets(resultados, soloAtendidos.Value ? "Tickets Atendidos" : "Tickets Pendientes");
            Validaciones.ReturnMenu();
        }

        private static void BuscarPorCliente()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Búsqueda por Cliente[/]").LeftJustified());
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Puede escribir el nombre completo o solo un nombre.[/]");
            AnsiConsole.WriteLine();

            string texto = Validaciones.LeerTexto("Nombre del cliente a buscar:");
            List<Ticket> resultados = gestor.BuscarPorCliente(texto);

            AnsiConsole.WriteLine();
            MostrarTablaTickets(resultados, $"Tickets del cliente \"{texto}\"");
            Validaciones.ReturnMenu();
        }

        private static void BuscarPorPalabraClave()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Búsqueda por Palabra Clave[/]").LeftJustified());
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Busca dentro de la descripción del problema\n" +
                                   "Si escribe varias palabras, el ticket debe contener todas.[/]");
            AnsiConsole.WriteLine();

            string texto = Validaciones.LeerTexto("Palabra clave a buscar:");
            List<Ticket> resultados = gestor.BuscarPorPalabraClave(texto);

            AnsiConsole.WriteLine();
            MostrarTablaTickets(resultados, $"Coincidencias para \"{texto}\"");
            Validaciones.ReturnMenu();
        }

        // Cantidad de tickets
        private static void MostrarCantidadTickets()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Cantidad de Tickets[/]").LeftJustified());
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]TOTAL DE TICKETS REGISTRADOS[/]");
            AnsiConsole.WriteLine();

            // Resumen general
            var resumen = new Grid();
            resumen.AddColumn(new GridColumn().NoWrap().PadRight(3));
            resumen.AddColumn(new GridColumn().RightAligned());
            resumen.AddRow("[bold]Tickets en cola (pendientes)[/]", $"[bold cyan]{gestor.CantidadEnCola}[/]");
            resumen.AddRow("[bold]Tickets atendidos[/]", $"[bold green]{gestor.CantidadAtendidos}[/]");
            resumen.AddRow("[bold]Total de tickets registrados[/]", $"[bold]{gestor.CantidadTotal}[/]");

            AnsiConsole.Write(new Panel(resumen)
            {
                Header = new PanelHeader(" Resumen General "),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("cyan"),
                Expand = false
            });
            AnsiConsole.WriteLine();

            // Distribución por prioridad
            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
            tabla.Title = new TableTitle("[bold]Distribución por Prioridad[/]");
            tabla.AddColumn("Prioridad");
            tabla.AddColumn(new TableColumn("En cola").RightAligned());
            tabla.AddColumn(new TableColumn("Atendidos").RightAligned());
            tabla.AddColumn(new TableColumn("Total").RightAligned());

            for (int p = 1; p <= 5; p++)
            {
                int enCola = gestor.ContarEnColaPorPrioridad(p);
                int atendidos = gestor.ContarAtendidosPorPrioridad(p);
                string c = EstiloPrioridad.MarkupDe(p);

                tabla.AddRow(
                    $"[bold {c}]{p} ({Ticket.NombrePrioridadDe(p)})[/]",
                    $"[{c}]{enCola}[/]",
                    $"[{c}]{atendidos}[/]",
                    $"[bold {c}]{enCola + atendidos}[/]");
            }

            AnsiConsole.Write(tabla);

            Validaciones.ReturnMenu();
        }

        // Reabrir ticket
        private static void ReabrirTicket()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Reabrir Ticket Atendido[/]").LeftJustified());
            AnsiConsole.WriteLine();

            Ticket? ultimo = gestor.ObtenerUltimoAtendido();
            if (ultimo == null)
            {
                AnsiConsole.MarkupLine("[yellow]No hay tickets atendidos para reabrir.[/]");
                Validaciones.ReturnMenu();
                return;
            }

            MostrarTablaTickets(gestor.ObtenerAtendidos(), "Tickets Atendidos");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]Presione Enter para reabrir el último ticket atendido ({ultimo.Codigo}).[/]");

            string entrada = Validaciones.LeerTexto("Código del ticket a reabrir:", permitirVacio: true);

            Ticket seleccionado;
            if (entrada.Length == 0)
            {
                seleccionado = ultimo;
            }
            else if (!Ticket.TryNormalizarCodigo(entrada, out string codigo))
            {
                AnsiConsole.MarkupLine("[red]Código inválido. Debe ser TCK seguido de 4 dígitos.[/]");
                Validaciones.ReturnMenu();
                return;
            }
            else
            {
                Ticket? encontrado = gestor.Buscar(codigo);
                if (encontrado == null)
                {
                    AnsiConsole.MarkupLine($"[red]No existe ningún ticket con el código {codigo}.[/]");
                    Validaciones.ReturnMenu();
                    return;
                }
                if (!encontrado.Atendido)
                {
                    AnsiConsole.MarkupLine($"[yellow]El ticket {codigo} ya se encuentra pendiente en la cola de prioridad.[/]");
                    Validaciones.ReturnMenu();
                    return;
                }
                seleccionado = encontrado;
            }

            AnsiConsole.WriteLine();
            MostrarDetalleTicket(seleccionado, "Ticket a Reabrir");
            AnsiConsole.WriteLine();

            if (!Validaciones.Confirmar($"¿Desea devolver el ticket {seleccionado.Codigo} a la cola de prioridad?"))
            {
                AnsiConsole.MarkupLine("[grey]Operación cancelada. El ticket continúa como atendido.[/]");
                Validaciones.ReturnMenu();
                return;
            }

            // se vuelve a insertar
            int posicion = gestor.Reabrir(seleccionado.Codigo);

            AnsiConsole.WriteLine();
            if (posicion < 0)
            {
                AnsiConsole.MarkupLine("[red]No se pudo reabrir el ticket.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[bold green]El ticket {seleccionado.Codigo} regresó a la cola como Pendiente.[/]");
            }

            Validaciones.ReturnMenu();
        }

        // Guardar ticket
        private static void GuardarDatos()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Guardar Datos[/]").LeftJustified());
            AnsiConsole.WriteLine();

            if (gestor.CantidadTotal == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No hay tickets registrados para guardar.[/]");
                Validaciones.ReturnMenu();
                return;
            }

            try
            {
                string ruta = GestorArchivos.Guardar(gestor.ObtenerTodos());

                AnsiConsole.MarkupLine(
                    $"[green]Se guardaron {gestor.CantidadTotal} tickets exitosamente " +
                    $"({gestor.CantidadEnCola} pendientes y {gestor.CantidadAtendidos} atendidos).[/]");
                AnsiConsole.MarkupLine($"[grey]Archivo: {Markup.Escape(ruta)}[/]");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                AnsiConsole.MarkupLine("[red]No se pudo guardar el archivo. Verifique que no esté abierto en otro programa.[/]");
                AnsiConsole.MarkupLine($"[grey]{Markup.Escape(ex.Message)}[/]");
            }

            Validaciones.ReturnMenu();
        }

        // Importar tickets
        private static void ImportarDatos()
        {
            MostrarEncabezado();
            AnsiConsole.Write(new Rule("[yellow]Importar Datos[/]").LeftJustified());
            AnsiConsole.WriteLine();

            if (!GestorArchivos.ExisteArchivo())
            {
                AnsiConsole.MarkupLine("[red]No se encontró el archivo 'tickets.csv' en la carpeta del programa.[/]");
                Validaciones.ReturnMenu();
                return;
            }

            EjecutarImportacion();
            Validaciones.ReturnMenu();
        }

        // Leer CSV
        private static void EjecutarImportacion()
        {
            List<Ticket> importados;
            List<string> errores;

            try
            {
                importados = GestorArchivos.Importar(out errores);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                AnsiConsole.MarkupLine("[red]No se pudo leer el archivo. Verifique que no esté abierto en otro programa.[/]");
                AnsiConsole.MarkupLine($"[grey]{Markup.Escape(ex.Message)}[/]");
                return;
            }

            int pendientes = 0, atendidos = 0, duplicados = 0;
            foreach (Ticket t in importados)
            {
                if (!gestor.Importar(t)) duplicados++;
                else if (t.Atendido) atendidos++;
                else pendientes++;
            }

            AnsiConsole.MarkupLine(
                $"[green]Importación finalizada: {pendientes + atendidos} tickets agregados " +
                $"({pendientes} pendientes y {atendidos} atendidos).[/]");

            if (duplicados > 0)
                AnsiConsole.MarkupLine($"[yellow]{duplicados} tickets fueron omitidos por tener un código ya existente.[/]");

            if (errores.Count > 0)
            {
                AnsiConsole.MarkupLine($"[red]{errores.Count} líneas no se pudieron procesar:[/]");
                foreach (string error in errores.Take(10))
                    AnsiConsole.MarkupLine($"  [red]- {Markup.Escape(error)}[/]");
            }
        }

        // Auto-import
        private static void Import()
        {
            if (!GestorArchivos.ExisteArchivo()) return;

            MostrarEncabezado();
            AnsiConsole.MarkupLine("[yellow]Se detectó un archivo de datos guardado anteriormente (tickets.csv).[/]");
            bool importar = Validaciones.Confirmar("¿Desea importar los tickets guardados antes de continuar?");

            if (!importar) return;

            EjecutarImportacion();
            Validaciones.ReturnMenu();
        }

        // datos ticket
        private static void MostrarDetalleTicket(Ticket t, string titulo)
        {
            string c = EstiloPrioridad.MarkupDe(t.Prioridad);

            var grid = new Grid();
            grid.AddColumn(new GridColumn().NoWrap().PadRight(1));
            grid.AddColumn();

            // Cada fila se muestra como "Etiqueta : valor"
            grid.AddRow("[bold]Código del Ticket[/]", $": [bold {c}]{Markup.Escape(t.Codigo)}[/]");
            grid.AddRow("[bold]Cliente[/]", $": {Markup.Escape(t.Cliente)}");
            grid.AddRow("[bold]Descripción[/]", $": {Markup.Escape(t.Descripcion)}");
            grid.AddRow("[bold]Prioridad[/]", $": [bold {c}]{Markup.Escape(t.PrioridadTexto)}[/]");
            grid.AddRow("[bold]Estado[/]", t.Atendido ? ": [grey]Atendido[/]" : ": [cyan]Pendiente[/]");
            grid.AddRow("[bold]Registrado[/]", $": {FormatearFecha(t.FechaRegistro)}");
            if (t.FechaAtencion.HasValue)
                grid.AddRow("[bold]Atendido el[/]", $": {FormatearFecha(t.FechaAtencion.Value)}");

            AnsiConsole.Write(new Panel(grid)
            {
                Header = new PanelHeader($" {titulo} "),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(EstiloPrioridad.ColorDe(t.Prioridad)),
                Padding = new Padding(2, 1),
                Expand = false
            });
        }

        // Tabla de tickets
        private static void MostrarTablaTickets(List<Ticket> tickets, string titulo)
        {
            if (tickets.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No se encontraron tickets que coincidan con el criterio ingresado.[/]");
                return;
            }

            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
            tabla.Title = new TableTitle($"[bold]{Markup.Escape(titulo)}[/]");
            tabla.AddColumn("Ticket");
            tabla.AddColumn("Cliente");
            tabla.AddColumn("Descripción");
            tabla.AddColumn("Prioridad");
            tabla.AddColumn("Estado");

            foreach (Ticket t in tickets)
            {
                string c = EstiloPrioridad.MarkupDe(t.Prioridad);

                tabla.AddRow(
                    $"[bold {c}]{Markup.Escape(t.Codigo)}[/]",
                    Markup.Escape(t.Cliente),
                    Markup.Escape(t.Descripcion),
                    $"[{c}]{Markup.Escape(t.PrioridadTexto)}[/]",
                    t.Atendido ? "[lightgreen]Atendido[/]" : "[cyan]Pendiente[/]");
            }

            AnsiConsole.Write(tabla);
            AnsiConsole.MarkupLine($"[grey]Total de resultados: {tickets.Count}[/]");
        }

        // tabla de prioridades
        private static void MostrarLeyendaPrioridades()
        {
            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            tabla.Title = new TableTitle("[bold]Niveles de Prioridad[/]");
            tabla.AddColumn("Prioridad");
            tabla.AddColumn("Nivel");
            tabla.AddColumn("Descripción");

            for (int p = 1; p <= 5; p++)
            {
                string c = EstiloPrioridad.MarkupDe(p);
                tabla.AddRow(
                    $"[bold {c}]{p}[/]",
                    $"[bold {c}]{Ticket.NombrePrioridadDe(p)}[/]",
                    Markup.Escape(Ticket.DescripcionPrioridadDe(p)));
            }

            AnsiConsole.Write(tabla);
        }

        private static void MostrarMensajeColaVacia()
        {
            AnsiConsole.MarkupLine("[yellow]La Cola de Prioridad se encuentra vacía. No hay tickets pendientes.[/]");
        }

        private static string FormatearFecha(DateTime fecha)
            => fecha.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
    }
}
