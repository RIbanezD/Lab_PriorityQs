using ColasDePrioridad_Ruben_Ibañez.Modelos;
using Spectre.Console;

namespace ColasDePrioridad_Ruben_Ibañez.Utilidades
{
    public static class Validaciones
    {
        public static string LeerTexto(string mensaje, bool permitirVacio = false)
        {
            while (true)
            {
                var prompt = new TextPrompt<string>($"[cyan]{mensaje}[/]") { AllowEmpty = true };
                string valor = AnsiConsole.Prompt(prompt).Trim();

                if (permitirVacio || valor.Length > 0)
                    return valor;

                AnsiConsole.MarkupLine("[red]Este campo no puede estar vacío. Intente nuevamente.[/]");
            }
        }

        public static int LeerEntero(string mensaje, int minimo, int maximo)
        {
            while (true)
            {
                string texto = LeerTexto(mensaje);

                if (int.TryParse(texto, out int valor) && valor >= minimo && valor <= maximo)
                    return valor;

                AnsiConsole.MarkupLine($"[red]Ingrese un número entero entre {minimo} y {maximo}.[/]");
            }
        }

        public static string? LeerCodigoTicket(string mensaje, bool permitirCancelar = false)
        {
            while (true)
            {
                string entrada = LeerTexto(mensaje, permitirVacio: permitirCancelar);

                if (entrada.Length == 0)
                    return null;

                if (Ticket.TryNormalizarCodigo(entrada, out string codigo))
                    return codigo;

                AnsiConsole.MarkupLine("[red]Código inválido. Debe ser TCK seguido de 4 dígitos (ejemplo: TCK0001).[/]");
            }
        }

        public static bool Confirmar(string mensaje)
        {
            while (true)
            {
                string respuesta = LeerTexto($"{mensaje} (s/n):").ToLowerInvariant();

                if (respuesta is "s" or "si" or "sí") return true;
                if (respuesta is "n" or "no") return false;

                AnsiConsole.MarkupLine("[red]Responda con 's' (sí) o 'n' (no).[/]");
            }
        }

        public static void ReturnMenu()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Presione cualquier tecla para regresar al menú...[/]");
            Console.ReadKey(true);
        }
    }
}
