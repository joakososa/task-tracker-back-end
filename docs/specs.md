# Especificaciones Funcionales — Task Tracker

> Requisitos generados con asistencia de IA (LLM) a partir de los lineamientos del proyecto (app tipo Trello, foco en practica). Son la base de trabajo para el desarrollo y se van a ir refinando/completando a medida que el proyecto avanza.
## Fase 1-3 — MVP (monolito)

### 1. Autenticación y usuarios

- **RF-01** Un usuario puede registrarse con email y contraseña.
- **RF-02** Un usuario puede iniciar sesión y recibir un JWT.
- **RF-03** Un usuario autenticado puede ver/editar su perfil básico (nombre, avatar opcional).

### 2. Proyectos

- **RF-04** Un usuario autenticado puede crear un proyecto (se convierte en *owner*).
- **RF-05** Un *owner* puede invitar/agregar otros usuarios al proyecto como miembros.
- **RF-06** Un usuario puede ver la lista de proyectos donde es miembro u *owner*.
- **RF-07** Un *owner* puede editar o eliminar su proyecto.
- **RF-08** Solo los miembros de un proyecto pueden ver su contenido (regla de autorización).

### 3. Tareas

- **RF-09** Un miembro del proyecto puede crear una tarea (título, descripción, prioridad).
- **RF-10** Una tarea tiene un estado: `Todo`, `InProgress`, `Done`. Se modela como enum; es un buen lugar para aplicar más adelante un patrón simple de máquina de estados (ver ADR correspondiente).
- **RF-11** Un miembro puede asignar una tarea a otro miembro del proyecto.
- **RF-12** Un miembro puede cambiar el estado de una tarea (ej: mover de `Todo` a `InProgress`).
- **RF-13** Un miembro del proyecto puede editar o eliminar una tarea 
- **RF-14** Se puede filtrar/listar las tareas de un proyecto por estado y asignado.

### 4. Comentarios

- **RF-15** Un miembro puede comentar en una tarea.
- **RF-16** Se listan los comentarios de una tarea ordenados por fecha.

### 5. Transversales

- **RF-17** Todas las escrituras (crear/editar/eliminar) quedan auditadas con `createdAt`, `updatedAt` y opcionalmente `createdBy`.
- **RF-18** Manejo de errores consistente (ej: `404` si la tarea no existe, `403` si no sos miembro del proyecto).

## Fases posteriores — Extensiones

### 6. Notificaciones

- **RF-19** Cuando se asigna una tarea a un usuario, se genera una notificación.
- **RF-20** Cuando alguien comenta en una tarea donde el usuario participa, se genera una notificación.
- **RF-21** El usuario puede ver sus notificaciones no leídas.

### 7. Tiempo real

- **RF-22** Los cambios de estado de tareas se reflejan en tiempo real a otros usuarios viendo el mismo proyecto (WebSockets/SignalR).

### 8. Reportes

- **RF-23** Generar un reporte semanal de actividad por proyecto (tareas creadas/cerradas, por usuario). Buen candidato para un servicio separado (posiblemente en Python) que consuma eventos del backend.

### 9. Caché

- **RF-24** Cachear el listado de proyectos de un usuario y el detalle de un proyecto, con invalidación al escribir.

## Trazabilidad

| Fase | Dominio | Requisitos |
|---|---|---|
| MVP | Autenticación y usuarios | RF-01 a RF-03 |
| MVP | Proyectos | RF-04 a RF-08 |
| MVP | Tareas | RF-09 a RF-14 |
| MVP | Comentarios | RF-15, RF-16 |
| MVP | Transversales | RF-17, RF-18 |
| Extensión | Notificaciones | RF-19 a RF-21 |
| Extensión | Tiempo real | RF-22 |
| Extensión | Reportes | RF-23 |
| Extensión | Caché | RF-24 |
