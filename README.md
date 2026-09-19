# Laboratorio 4 — Cola de Prioridad con Min Heap en C#

Sistema de gestión de tickets de soporte de **TechSolutions**, desarrollado como aplicación de consola en C#.
El ticket más urgente siempre está en la raíz.

## Estructura del proyecto

```
ColasDePrioridad_Ruben_Ibañez/
├── Program.cs                 Menú principal, submenús y pantallas
├── Modelos/
│   └── Ticket.cs               Modelo de datos del ticket
├── Estructuras/
│   └── MinHeap.cs               Implementación propia del Min Heap (Heap Up / Heap Down)
├── Servicios/
│   ├── GestorTickets.cs         Coordina el heap (pendientes) y el historial (atendidos)
│   └── GestorArchivos.cs        Guardar / importar CSV
└── Utilidades/
    ├── Validaciones.cs          Lectura y validación de entradas
    └── EstiloPrioridad.cs       Colores de cada nivel de prioridad
```

## Funcionalidades

| Opción | Descripción |
|---|---|
| 1. Registrar Ticket | Código con correlativo **automático** o **manual** (TCK + 4 dígitos). |
| 2. Mostrar Siguiente Ticket |
| 3. Atender Ticket |
| 4. Mostrar Cola de Prioridad | Tabla con colores por prioridad (rojo = más urgente, verde = menos urgente). |
| 5. Buscar Ticket | Por código, prioridad, estado (pendientes / atendidos), cliente o descripción. |
| 6. Mostrar Cantidad de Tickets | En cola, atendidos y total. |
| 7. Reabrir Ticket Atendido | Devuelve un ticket atendido. |
| 8. Guardar Datos | Exporta todos los tickets a `tickets.csv`. |
| 9. Importar Datos | Carga los tickets desde `tickets.csv`. |
| 10. Salir | |

## Notas

- **Spectre.Console** se usa únicamente con fines estéticos.
- Al guardar datos se crea **tickets.csv** en la carpeta del ejecutable. Al volver a ejecutar el programa, si ese
  archivo existe, se ofrece importarlo automáticamente.
- El código de un ticket es único entre pendientes **y** atendidos.
- **Criterio del heap:** menor número de prioridad primero. Si dos tickets tienen la misma prioridad, se atiende
  primero el que se registró antes.
- Al reabrir un ticket se conserva su fecha de registro original, por lo que recupera su lugar correcto en la cola.