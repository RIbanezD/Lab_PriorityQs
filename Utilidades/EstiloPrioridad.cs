using Spectre.Console;

namespace ColasDePrioridad_Ruben_Ibañez.Utilidades
{
    public static class EstiloPrioridad
    {
        public static Color ColorDe(int prioridad) => prioridad switch
        {
            1 => Color.Red,
            2 => Color.DarkOrange,
            3 => Color.Yellow,
            4 => Color.Chartreuse1,
            _ => Color.Green
        };

        public static string MarkupDe(int prioridad) => prioridad switch
        {
            1 => "red",
            2 => "darkorange",
            3 => "yellow",
            4 => "chartreuse1",
            _ => "green"
        };
    }
}
