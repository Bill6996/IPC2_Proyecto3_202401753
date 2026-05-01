# IPC2_Proyecto3_202401753

**Universidad de San Carlos de Guatemala**  
Facultad de Ingeniería — Escuela de Ciencias y Sistemas  
Introducción a la Programación y Computación 2  

**Estudiante:** Bily Vallecidos Folgar
**Carnet:** 202401753  
**Catedráticos:** Inga. Claudia Liceth Rojas Morales, 
Auxiliar: Diego

---

## Descripción general

Este proyecto implementa el sistema de facturación y pagos de la empresa ficticia **Industria Típica Guatemalteca, S.A. (ITGSA)**. El sistema se divide en dos programas independientes que se comunican mediante el protocolo HTTP:

- **Backend (Servicio 2):** API REST desarrollada en .NET que expone endpoints HTTP, procesa archivos XML de entrada y persiste todos los datos en archivos XML locales.
- **Frontend (Programa 1):** Aplicación web desarrollada con Razor Pages que actúa como interfaz de usuario, consume el API del backend y presenta los datos de forma visual.

El sistema permite registrar clientes y bancos, procesar facturas generadas por una tienda virtual, registrar pagos provenientes de bancas en línea, y aplicar los pagos a las facturas pendientes más antiguas de cada cliente. Si un pago excede el total de deuda de un cliente, el excedente queda registrado como saldo a favor y se aplica automáticamente en la próxima factura.

---

## Requisitos previos

- .NET SDK 8.0 o superior
- Visual Studio 2022 o Visual Studio Code con extensión de C#
- Git

Para verificar la instalación de .NET:

```
dotnet --version
```

---

## Instalación y ejecución

**1. Clonar el repositorio**

```
git clone https://github.com/[TU_USUARIO]/IPC2_Proyecto3_202401753.git
cd IPC2_Proyecto3_202401753
```

**2. Restaurar dependencias**

```
dotnet restore
```

**3. Ejecutar el Backend**

Abrir una terminal en la carpeta raíz del proyecto:

```
cd Backend
dotnet run
```

El backend queda disponible en `http://localhost:5001`. La documentación interactiva Swagger se encuentra en `http://localhost:5001/swagger`.

**4. Ejecutar el Frontend**

Abrir una segunda terminal:

```
cd Frontend
dotnet run
```

La interfaz web queda disponible en `http://localhost:5000`.

**Nota:** El backend debe estar en ejecución antes de usar el frontend. Ambos procesos deben correr simultáneamente.

---

## Arquitectura del sistema

```
Frontend (Razor Pages)          Backend (API REST)
http://localhost:5000    <--->  http://localhost:5001
        |                               |
   Razor Pages                   Controllers
   ApiService                    Services
        |                        Models (POO)
        |                               |
   HTTP Requests              Archivos XML
   (multipart, JSON)          (persistencia local)
```

La comunicación entre ambos programas se realiza exclusivamente mediante HTTP. El frontend nunca accede directamente a los archivos XML; toda operación pasa por el API del backend.

---

## Estructura del proyecto

```
IPC2_Proyecto3_202401753/
|
|-- Backend/
|   |-- Controllers/
|   |   `-- ITGSAController.cs
|   |-- Models/
|   |   |-- Cliente.cs
|   |   |-- Banco.cs
|   |   |-- Factura.cs
|   |   |-- Pago.cs
|   |   `-- XmlWrappers.cs
|   |-- Services/
|   |   |-- XmlDataService.cs
|   |   |-- ConfigService.cs
|   |   |-- TransaccionService.cs
|   |   |-- EstadoCuentaService.cs
|   |   `-- PdfService.cs
|   |-- Data/
|   |   |-- clientes.xml
|   |   |-- bancos.xml
|   |   |-- facturas.xml
|   |   `-- pagos.xml
|   `-- Program.cs
|
|-- Frontend/
|   |-- Pages/
|   |   |-- Shared/
|   |   |   `-- _Layout.cshtml
|   |   |-- Ayuda/
|   |   |   |-- Ayuda.cshtml / Ayuda.cshtml.cs
|   |   |   `-- Documentacion.cshtml / Documentacion.cshtml.cs
|   |   |-- EstadoCuenta/
|   |   |   `-- Pdf.cshtml / Pdf.cshtml.cs
|   |   |-- Ingresos/
|   |   |   `-- Pdf.cshtml / Pdf.cshtml.cs
|   |   |-- Configuracion.cshtml / Configuracion.cshtml.cs
|   |   |-- Transacciones.cshtml / Transacciones.cshtml.cs
|   |   |-- EstadoCuenta.cshtml / EstadoCuenta.cshtml.cs
|   |   |-- Ingresos.cshtml / Ingresos.cshtml.cs
|   |   |-- Reset.cshtml / Reset.cshtml.cs
|   |   |-- Index.cshtml / Index.cshtml.cs
|   |   |-- Error.cshtml / Error.cshtml.cs
|   |   `-- Ayuda.cshtml / Ayuda.cshtml.cs
|   |-- Services/
|   |   `-- ApiService.cs
|   `-- Program.cs
|
|-- Pruebas/
|   |-- config.xml
|   |-- config2.xml
|   |-- transac.xml
|   `-- transac2.xml
|
`-- README.md
```

---

## Backend

### Program.cs

Punto de entrada del backend. Configura el contenedor de inyección de dependencias, registra todos los servicios, habilita Swagger para la documentación interactiva del API, y configura CORS para permitir solicitudes desde el frontend en el puerto 5000.

Los servicios se registran con los siguientes ciclos de vida:
- `XmlDataService` como Singleton: existe una sola instancia durante toda la vida del proceso, lo que garantiza acceso sincronizado a los archivos XML.
- `ConfigService`, `TransaccionService`, `EstadoCuentaService` y `PdfService` como Scoped: se crea una nueva instancia por cada solicitud HTTP.

---

### Modelos (carpeta Models)

Los modelos representan las entidades del dominio del sistema. Todos implementan el paradigma de programación orientada a objetos y utilizan atributos de serialización XML para mapear las propiedades a elementos del archivo.

#### Cliente.cs

Representa a un cliente registrado en el sistema. Sus propiedades son:

- `NIT` (string): identificador único del cliente. Puede contener letras, números y guiones.
- `Nombre` (string): nombre o razón social del cliente.
- `SaldoAFavor` (double): monto acumulado de pagos que excedieron las facturas del cliente. Este valor se aplica automáticamente a la siguiente factura que genere el cliente.

#### Banco.cs

Representa a un banco registrado en el sistema. Sus propiedades son:

- `Codigo` (int): identificador numérico único del banco.
- `Nombre` (string): nombre del banco.

#### Factura.cs

Representa una factura generada por la tienda virtual. Sus propiedades son:

- `NumeroFactura` (string): identificador único de la factura.
- `NITcliente` (string): NIT del cliente al que pertenece la factura.
- `Fecha` (string): fecha de emisión en formato dd/MM/yyyy.
- `Valor` (double): monto original de la factura.
- `SaldoPendiente` (double): monto que aún no ha sido cubierto por pagos. Comienza igual a `Valor` y se reduce conforme llegan pagos del cliente.

#### Pago.cs

Representa un pago realizado por un cliente a través de su banca en línea. Sus propiedades son:

- `CodigoBanco` (int): código del banco desde el que se realizó el pago.
- `Fecha` (string): fecha del pago en formato dd/MM/yyyy.
- `NITcliente` (string): NIT del cliente que realizó el pago.
- `Valor` (double): monto del pago.

#### XmlWrappers.cs

Define las clases contenedoras que se utilizan para serializar y deserializar las listas de entidades hacia y desde archivos XML. Cada clase envuelve una lista de entidades y define el elemento raíz del documento XML correspondiente:

- `ClienteList`: envuelve `List<Cliente>`, raíz `<clientes>`.
- `BancoList`: envuelve `List<Banco>`, raíz `<bancos>`.
- `FacturaList`: envuelve `List<Factura>`, raíz `<facturas>`.
- `PagoList`: envuelve `List<Pago>`, raíz `<pagos>`.

---

### Servicios (carpeta Services)

#### XmlDataService.cs

Servicio de acceso a datos. Es el único componente del sistema que lee y escribe los archivos XML de persistencia. Está registrado como Singleton para garantizar que todas las solicitudes compartan la misma instancia y evitar condiciones de carrera mediante un objeto de bloqueo (`lock`).

Responsabilidades:
- Inicializar los cuatro archivos XML al arrancar el backend si no existen.
- Exponer métodos `Get` y `Save` para cada entidad: `GetClientes`, `SaveClientes`, `GetBancos`, `SaveBancos`, `GetFacturas`, `SaveFacturas`, `GetPagos`, `SavePagos`.
- Exponer el método `LimpiarTodo` que sobrescribe todos los archivos con listas vacías, utilizado por el endpoint de reset.
- Manejar errores de lectura devolviendo instancias vacías en lugar de propagar excepciones.

#### ConfigService.cs

Servicio encargado de procesar el archivo `config.xml`. Recibe el contenido XML como cadena de texto, lo parsea con `XDocument` y aplica la lógica de creación o actualización de clientes y bancos.

Lógica principal:
- Por cada elemento `<cliente>`, limpia el NIT usando la expresión regular `[^a-zA-Z0-9\-]` para eliminar caracteres no válidos. Si ya existe un cliente con ese NIT, actualiza su nombre (actualizado). Si no existe, lo crea (creado).
- Por cada elemento `<banco>`, valida que el código sea numérico. Si ya existe un banco con ese código, actualiza su nombre. Si no existe, lo crea.
- Devuelve cuatro contadores: clientes creados, clientes actualizados, bancos creados, bancos actualizados.

El método estático `LimpiarNIT` está disponible para uso compartido con `TransaccionService`.

#### TransaccionService.cs

Servicio encargado de procesar el archivo `transac.xml`. Es el servicio más complejo del sistema, ya que implementa la lógica central de facturación y pagos.

**Procesamiento de facturas:**

Por cada elemento `<factura>` en el XML:
1. Limpia el texto del número de factura y el NIT del cliente.
2. Extrae la fecha usando la expresión regular `\b(\d{2}/\d{2}/\d{4})\b`, lo que permite ignorar texto extra alrededor de la fecha.
3. Limpia el valor numérico eliminando caracteres no numéricos con `[^\d.,]`.
4. Verifica que el número de factura no esté vacío; si lo está, cuenta como error.
5. Verifica si ya existe una factura con ese número; si existe, cuenta como duplicada.
6. Verifica que el cliente exista en la base de datos; si no existe, cuenta como error.
7. Valida el formato de la fecha; si es inválida, cuenta como error.
8. Valida que el valor sea numérico y mayor a cero; si no, cuenta como error.
9. Si el cliente tiene saldo a favor, lo aplica al saldo pendiente de la nueva factura antes de guardarla.
10. Guarda la factura con su saldo pendiente calculado.

**Procesamiento de pagos:**

Por cada elemento `<pago>` en el XML:
1. Valida que el código de banco sea numérico y exista en la base de datos.
2. Valida que el cliente exista.
3. Extrae y valida la fecha.
4. Valida el valor numérico.
5. Verifica que no sea un pago duplicado (mismo banco, cliente, fecha y monto con tolerancia de 0.001).
6. Aplica el pago a las facturas pendientes del cliente ordenadas de más antigua a más reciente, abonando el máximo posible a cada una.
7. Si queda remanente después de cubrir todas las facturas, lo suma al saldo a favor del cliente.
8. Guarda el pago en la base de datos.

**Métodos auxiliares con expresiones regulares:**

- `ExtraerFecha`: usa `\b(\d{2}/\d{2}/\d{4})\b` para extraer una fecha válida de un texto que pueda contener información adicional como nombres de ciudades.
- `LimpiarNumero`: usa `[^\d.,]` para eliminar caracteres no numéricos de valores monetarios.
- `LimpiarTexto`: usa `\s+` para normalizar espacios múltiples en cadenas de texto.
- `TryParseFecha`: intenta parsear una fecha en formato `dd/MM/yyyy` usando `CultureInfo.InvariantCulture`.

#### EstadoCuentaService.cs

Servicio de consulta que genera los datos para las dos secciones de peticiones del frontend.

**Método `GetEstadoCuenta(string? nit)`:**

Construye el estado de cuenta de uno o todos los clientes. Para cada cliente:
- Reúne todas sus facturas como transacciones de tipo "cargo".
- Reúne todos sus pagos como transacciones de tipo "abono", resolviendo el nombre del banco correspondiente.
- Ordena las transacciones de más reciente a más antigua.
- Calcula el saldo pendiente total sumando los saldos pendientes de todas sus facturas.
- Devuelve un objeto con NIT, nombre, saldo pendiente, saldo a favor, total de facturas, total de pagos y la lista de transacciones.

Si el parámetro `nit` es nulo o vacío, devuelve todos los clientes ordenados por NIT.

**Método `GetResumenPagos(int mes, int anio)`:**

Genera el resumen de ingresos por banco para los últimos tres meses a partir del mes y año indicados. Para cada banco registrado, calcula el total recaudado en cada uno de los tres meses. Los meses se calculan de forma regresiva, manejando correctamente el cruce de año (por ejemplo, enero 2024 retrocede a diciembre 2023).

#### PdfService.cs

Servicio que genera documentos PDF utilizando la librería QuestPDF con licencia Community. Recibe los datos de los otros servicios y los convierte en documentos descargables con formato profesional.

**Método `GenerarEstadoCuentaPdf(string? nit)`:**

Genera un PDF con el estado de cuenta de uno o todos los clientes. El documento incluye:
- Encabezado con el nombre de la empresa y datos del curso.
- Por cada cliente: encabezado con NIT y nombre, indicadores de saldo pendiente y saldo a favor, y tabla de transacciones con fecha, descripción y monto.
- Pie de página con nombre de la empresa y numeración de páginas.

**Método `GenerarResumenPagosPdf(int mes, int anio)`:**

Genera un PDF con el resumen de ingresos por banco. Incluye:
- Tabla con una fila por banco y una columna por cada uno de los tres meses del período.
- Fila de totales por columna y total general.
- Resumen textual por mes con cantidad de pagos y total recaudado.

**Método privado `ComposeHeader`:**

Construye el encabezado compartido entre ambos tipos de PDF, con el nombre de la empresa a la izquierda y los datos del curso a la derecha.

---

### Controlador (carpeta Controllers)

#### ITGSAController.cs

Controlador principal del API. Expone once endpoints HTTP bajo la ruta base `/api`. Recibe las dependencias por inyección de constructor y delega toda la lógica de negocio a los servicios correspondientes.

**Endpoints disponibles:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | /api/grabarConfiguracion | Recibe un archivo config.xml como multipart/form-data y devuelve un XML con contadores de creados y actualizados |
| POST | /api/grabarTransaccion | Recibe un archivo transac.xml como multipart/form-data y devuelve un XML con contadores de nuevos, duplicados y errores |
| POST | /api/limipiarDatos | Resetea todos los archivos XML a estado vacío |
| GET | /api/devolverEstadoCuenta | Devuelve el estado de cuenta. Acepta parámetro opcional `?nit=` para filtrar por cliente |
| GET | /api/devolverResumenPagos | Devuelve el resumen de ingresos por banco. Requiere parámetros `?mes=` y `?anio=` |
| GET | /api/pdfEstadoCuenta | Genera y descarga el PDF del estado de cuenta. Acepta `?nit=` opcional |
| GET | /api/pdfResumenPagos | Genera y descarga el PDF del resumen de pagos. Requiere `?mes=` y `?anio=` |
| GET | /api/clientes | Devuelve la lista de todos los clientes ordenados por NIT |
| GET | /api/bancos | Devuelve la lista de todos los bancos ordenados por código |
| GET | /api/facturas | Devuelve facturas. Acepta `?nit=` para filtrar por cliente |
| GET | /api/pagos | Devuelve pagos. Acepta `?nit=` para filtrar por cliente |
| GET | /api/estadisticas | Devuelve contadores y totales monetarios globales del sistema |

Todos los endpoints que reciben o devuelven XML utilizan `Content-Type: application/xml`. Los endpoints de consulta devuelven JSON. En caso de error, todos los endpoints devuelven un XML con la estructura `<error><mensaje>...</mensaje></error>`.

---

## Frontend

### Program.cs

Punto de entrada del frontend. Registra los servicios de Razor Pages, configura el `HttpClient` con la URL base del backend leída desde `appsettings.json`, y registra `ApiService` como servicio Scoped.

### ApiService.cs

Capa de acceso al API del backend. Encapsula todas las llamadas HTTP para que las páginas Razor no dependan directamente de `HttpClient`. Expone un método por cada endpoint del backend:

- `GrabarConfiguracion`: construye un `MultipartFormDataContent` con el archivo recibido y lo envía al endpoint correspondiente.
- `GrabarTransaccion`: mismo comportamiento que el anterior para el archivo de transacciones.
- `LimpiarDatos`: realiza un POST sin cuerpo al endpoint de limpieza.
- `GetEstadoCuenta`: realiza un GET con el parámetro NIT opcional.
- `GetResumenPagos`: realiza un GET con mes y año.
- `DescargarPdfEstadoCuenta`: realiza un GET y devuelve los bytes del PDF para que la página los sirva como descarga.
- `DescargarPdfResumenPagos`: mismo comportamiento para el PDF de ingresos.
- `GetClientes`, `GetBancos`: devuelven las listas en formato JSON.
- `GetEstadisticas`: devuelve los contadores y totales globales.

---

### Páginas Razor (carpeta Pages)

#### _Layout.cshtml

Plantilla base compartida por todas las páginas. Define la estructura HTML completa incluyendo el navbar superior con el nombre del sistema y el enlace a Swagger, el sidebar de navegación lateral con resaltado de la página activa calculado comparando la ruta actual con cada enlace, el contenedor de alertas globales que lee mensajes desde `TempData`, y las referencias a Bootstrap 5, Bootstrap Icons y Chart.js cargadas desde CDN.

#### Index.cshtml / Index.cshtml.cs

Panel de control principal. Al cargar, el PageModel llama a `ApiService.GetEstadisticas()` y deserializa la respuesta en un objeto `StatsVM`. La página muestra ocho indicadores numéricos: cantidad de clientes, bancos, facturas y pagos, y totales monetarios de facturado, pagado, pendiente y saldo a favor. Debajo presenta tarjetas de acceso directo a cada sección del sistema.

#### Configuracion.cshtml / Configuracion.cshtml.cs

Página para cargar el archivo `config.xml`. Presenta una zona de carga visual que al hacer clic activa el input de archivo oculto. Al enviar el formulario, el PageModel llama a `ApiService.GrabarConfiguracion`, parsea el XML de respuesta con `XDocument` y extrae los cuatro contadores para mostrarlos en tarjetas. Un spinner de carga se activa mediante JavaScript al momento del envío para dar retroalimentación visual. También muestra el XML de respuesta completo en un bloque de código formateado.

#### Transacciones.cshtml / Transacciones.cshtml.cs

Página para cargar el archivo `transac.xml`. Funciona de forma análoga a la página de configuración. Muestra seis contadores agrupados en dos secciones: facturas (nuevas, duplicadas, con error) y pagos (nuevos, duplicados, con error). Incluye el formato esperado del archivo como referencia.

#### EstadoCuenta.cshtml / EstadoCuenta.cshtml.cs

Página de consulta de estado de cuenta. Presenta un campo de búsqueda por NIT que si se deja vacío devuelve todos los clientes. Al enviar la consulta, llama a `GetEstadoCuenta`, deserializa la lista de `ClienteVM` y por cada cliente muestra su encabezado con NIT, nombre y badges de saldo, seguido de una tabla de transacciones ordenadas de más reciente a más antigua, diferenciando visualmente cargos (rojo) y abonos (verde). Incluye un botón para descargar el PDF correspondiente que redirige a `EstadoCuenta/Pdf`.

#### EstadoCuenta/Pdf.cshtml.cs

PageModel auxiliar que llama a `ApiService.DescargarPdfEstadoCuenta` y retorna el resultado como `FileResult` con tipo MIME `application/pdf`, lo que provoca que el navegador descargue el archivo directamente.

#### Ingresos.cshtml / Ingresos.cshtml.cs

Página de visualización de ingresos por banco. Presenta un formulario de selección de mes y año. Al consultar, llama a `GetResumenPagos`, deserializa la lista de `BancoIngresosVM` y la serializa nuevamente a JSON para inyectarla en el bloque de scripts de la página. El script de JavaScript utiliza Chart.js para renderizar una gráfica de barras agrupadas con un dataset por cada uno de los tres meses del período. También genera una tabla de datos con los mismos valores para referencia. Incluye un botón para descargar el PDF del período seleccionado.

#### Ingresos/Pdf.cshtml.cs

PageModel auxiliar análogo al de EstadoCuenta que descarga el PDF de ingresos.

#### Reset.cshtml / Reset.cshtml.cs

Página de reset del sistema con confirmación en dos pasos. El primer envío del formulario activa el estado `Confirmando = true` mediante el handler `OnPostConfirmar`, que muestra un segundo formulario de confirmación con advertencia explícita. El segundo envío llama a `OnPostEjecutarAsync` que invoca `ApiService.LimpiarDatos` y muestra el mensaje de confirmación. Este flujo previene resets accidentales.

#### Ayuda.cshtml / Ayuda.cshtml.cs

Página de ayuda con dos pestañas implementadas con Bootstrap Tab:
- **Información del estudiante:** tabla con nombre, carnet, curso, facultad, universidad y enlace al repositorio, además del listado de catedráticos e instructores del curso.
- **Manual del sistema:** tarjetas explicativas sobre la arquitectura, el formato de los archivos de entrada, la lógica de pagos y las reglas de validación y errores, más una tabla completa de todos los endpoints del API.

#### Ayuda/Documentacion.cshtml / Documentacion.cshtml.cs

Página de documentación técnica del sistema. Presenta:
- Diagrama de capas del sistema mostrando Frontend, capa HTTP, Backend y persistencia XML.
- Lista de los cuatro modelos con sus propiedades y tipos.
- Lista de los cinco servicios del backend con su ciclo de vida y descripción.
- Tabla de las cuatro expresiones regulares utilizadas con ejemplos de entrada y salida.

#### Error.cshtml / Error.cshtml.cs

Página de error personalizada. Se muestra cuando ocurre un error no controlado o cuando el backend no está disponible. Presenta opciones para volver al inicio o abrir la documentación Swagger para verificar el estado del API.

---

## Archivos de persistencia (Backend/Data)

El backend utiliza cuatro archivos XML como base de datos. Se crean automáticamente al iniciar el backend si no existen.

- **clientes.xml:** lista de clientes con NIT, nombre y saldo a favor.
- **bancos.xml:** lista de bancos con código y nombre.
- **facturas.xml:** lista de facturas con número, NIT del cliente, fecha, valor original y saldo pendiente.
- **pagos.xml:** lista de pagos con código del banco, fecha, NIT del cliente y valor.

Todos los archivos son sobrescritos completamente en cada operación de escritura. El acceso concurrente está protegido por un objeto de bloqueo en `XmlDataService`.

---

## Archivos de prueba (carpeta Pruebas)

La carpeta `Pruebas` contiene archivos XML de ejemplo para probar el sistema de forma integral.

- **config.xml:** registra cinco clientes y cuatro bancos.
- **config2.xml:** archivo incremental que actualiza un cliente existente y agrega uno nuevo, y agrega un banco nuevo y actualiza uno existente. Sirve para verificar el comportamiento de actualización del endpoint de configuración.
- **transac.xml:** registra ocho facturas y nueve pagos para distintos clientes, incluyendo un caso de factura con NIT inexistente (debe contar como error), una factura duplicada (mismo número de factura), y un pago con código de banco inexistente (debe contar como error).
- **transac2.xml:** archivo incremental con dos facturas nuevas y un pago que genera saldo a favor, para verificar que el sistema aplica correctamente el excedente.

**Flujo de prueba recomendado:**

1. Cargar `config.xml` — resultado esperado: 5 clientes creados, 4 bancos creados.
2. Cargar `transac.xml` — resultado esperado: 8 facturas nuevas, 1 duplicada, 1 con error; 8 pagos nuevos, 1 con error.
3. Cargar `config2.xml` — resultado esperado: 1 cliente creado, 1 actualizado; 1 banco creado, 1 actualizado.
4. Cargar `transac2.xml` — resultado esperado: 2 facturas nuevas; 1 pago nuevo con saldo a favor generado.
5. Consultar estado de cuenta del cliente `987654-3` — debe mostrar saldo a favor.
6. Consultar ingresos para marzo/2024 — debe mostrar datos de los cuatro bancos.

---

## Dependencias externas

**Backend:**

| Paquete | Version | Uso |
|---------|---------|-----|
| Swashbuckle.AspNetCore | Incluido en .NET 8 | Generación de documentación Swagger |
| QuestPDF | 2024.x | Generación de documentos PDF |

**Frontend:**

| Recurso | Versión | Uso |
|---------|---------|-----|
| Bootstrap | 5.3.3 | Framework CSS para estilos y componentes |
| Bootstrap Icons | 1.11.3 | Librería de iconos SVG |
| Chart.js | 4.4.3 | Generación de gráficas de barras |

Bootstrap, Bootstrap Icons y Chart.js se cargan desde CDN en el layout principal y no requieren instalación local.

---

## Expresiones regulares utilizadas

El sistema utiliza expresiones regulares en `ConfigService` y `TransaccionService` para limpiar y extraer información de los datos de entrada, siguiendo el requerimiento del enunciado de ignorar información extra en lugar de marcarla como error.

| Expresión | Clase | Propósito |
|-----------|-------|-----------|
| `[^a-zA-Z0-9\-]` | ConfigService | Elimina cualquier caracter que no sea alfanumérico o guión de los NITs |
| `\b(\d{2}/\d{2}/\d{4})\b` | TransaccionService | Extrae el patrón de fecha dd/mm/yyyy de textos que puedan contener información adicional |
| `[^\d.,]` | TransaccionService | Elimina caracteres no numéricos de valores monetarios, permitiendo punto y coma como separadores decimales |
| `\s+` | TransaccionService | Normaliza secuencias de espacios múltiples en cadenas de texto a un solo espacio |

---

## Principios de diseño aplicados

**Programación orientada a objetos:** cada entidad del dominio está representada por una clase con propiedades encapsuladas. Los servicios agrupan comportamiento relacionado y las dependencias se inyectan mediante constructor, sin acoplamiento directo entre clases.

**Separación de responsabilidades:** el controlador únicamente recibe solicitudes HTTP y construye respuestas. La lógica de negocio reside en los servicios. El acceso a datos está encapsulado en `XmlDataService`. El frontend no contiene lógica de negocio; toda operación se delega al backend.

**Inyección de dependencias:** todos los servicios se registran en el contenedor de .NET y se resuelven automáticamente. Ninguna clase instancia directamente sus dependencias con `new`.

**Ciclo de vida de servicios:** `XmlDataService` es Singleton para garantizar consistencia en el acceso a los archivos. Los servicios de negocio son Scoped para aislar el estado entre solicitudes.

---

## Versionamiento

El proyecto fue desarrollado mediante cuatro releases publicados en GitHub:

- **v1.0.0 — Release 1:** estructura base del proyecto, cinco endpoints del API, todas las páginas Razor, persistencia XML, lógica completa de pagos con saldo a favor.
- **v2.0.0 — Release 2:** generación de PDFs con QuestPDF, tabla de datos en la sección de ingresos, panel con estadísticas monetarias, reset con confirmación en dos pasos, cuatro endpoints adicionales.
- **v3.0.0 — Release 3:** validaciones avanzadas con expresiones regulares, archivos XML de prueba, sidebar con navegación activa, página de ayuda con pestañas, página de documentación técnica.
- **v4.0.0 — Release 4:** endpoint de estadísticas globales, mejoras de experiencia de usuario con spinner de carga y zona de arrastre para archivos, página de error personalizada.
