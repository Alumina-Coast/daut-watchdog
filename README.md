# DAUT-Watchdog

Aplicación para monitorear carpetas y alertar por email en condiciones particulares.

## Instalación

Instale la versión publicada de DAUT-Watchdog como un servicio de Windows, utilizando los siguientes comandos:

```sh
sc create "Service Name" binPath="path\to\binary.exe" start= auto
```

Es probable que deba iniciarlo manualmente desde la página de gestión de servicios la primera vez.

## Configuración

Para configurar el programa, debe modificar el archivo `config.yaml` ubicado en el mismo directorio que el ejecutable.

### Ejemplo de Configuración

```yaml
IterarCada: 1.00:00:00
Guardias:
  - Nombre: test
    Directorio: C:\test
    IncluirSubdirectorios: true
    Filtros:
      - "texto.txt"
      - "*.log\"
      - "nombre.*"
    Condiciones:
      - Condicion: Inactividad
	TiempoLimite: 0.00:01:00
      - Condicion: UltimaLinea
        Contiene: error
  - Nombre: TEST2
    Directorio: C:\test
    Condiciones:
      - Condicion: Inactividad
        TiempoLimite: 0.01:00:00
ConfiguracionEmail:
  FromEmail: sender@example.com
  Subject: TEST
  EmailHeader: << TOP OF EMAIL >>
  SmtpServer: smtp.example.com
  SmtpPort: 587
  Username: username
  Password: password123
  UseSsl: false
  UseCredentials: false
DireccionesEmail:
  - recipient@example.com
  - recipient2@example.com
```
	  
### Descripción de Parámetros
	  
- **IterarCada**: Tiempo de espera entre chequeo de guardias.
- **Guardias**: Lista de guardias configurados.
- **Nombre**: Identificador del guardia.
- **Directorio**: Ubicación a monitorear.
- **IncluirSubdirectorios**: Flag para incluir subdirectorios. Falso por defecto.
- **Filtros**: Filtros o nombres de archivos a monitorear. De no incluir ninguno, se monitorean todos los archivos de la carpeta.
- **Condiciones**: Lista de condiciones a chequear.
- **Condicion**: Tipo de condición. Referirse a condiciones.
- **Unanime**: Si es verdadero, todos los archivos que sean chequeados por el guardia deben cumplir esta condición para lanzar una alerta.
- **ConfiguracionEmail**: Configuración SMTP.
- **FromEmail**: Email de origen.
- **Subject**: Asunto para el email autogenerado.
- **EmailHeader**: Texto para incluir antes del cuerpo de los emails autogenerados.
- **SmtpServer**: Dirección del servidor SMTP.
- **SmtpPort**: Puerto del servidor SMTP.
- **Username**: Credenciales SMTP.
- **Password**: Credenciales SMTP.
- **UseSsl**: Conectar al servidor SMTP por SSL.
- **UseCredentials**: Intentar iniciar sesión en el servidor SMTP.
- **DireccionesEmail**: Lista de emails a donde enviar las alertas.
	  
Utilice \"-\" para las listas y respete el espaciado. Si el programa no inicializa después de modificar la configuración, verifique que esté respetando el formato YAML.
	  
## Condiciones
	  
### Inactividad
	  
Se cumple si los archivos no han sido modificados por cierto tiempo.
- **TiempoLimite**: Tiempo límite.
	  
### UltimaLinea
	  
Se cumple si la última línea de un archivo modificado contiene cierto texto.
- **Contiene**: Cadena de texto a verificar.

---

