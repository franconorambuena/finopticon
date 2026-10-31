# 📊 Finopticon
<img width="978" height="629" alt="Image" src="https://github.com/user-attachments/assets/0c3bc698-f5e0-422c-b5dc-d0600c3695e7" />
> **Gestor financiero personal local, ultraligero y privado**, desarrollado en C# utilizando Windows Forms. Diseñado para funcionar 100% offline sin dependencias externas pesadas.

---

## ✨ Características Principales

* 🔒 **Privacidad Total:** Tus datos se guardan localmente en un archivo JSON cifrado/estructurado en tu propio equipo.
* ⚡ **Sin Instaladores Pesados:** Compilado de forma nativa mediante el compilador de .NET (`csc.exe`).
* 🎨 **Interfaz Moderna:** Diseño personalizado en modo oscuro (*Dark Mode*) con pestañas intuitivas para movimientos y balances generales.
* 🔍 **Filtros y Búsqueda en Vivo:** Encuentra rápidamente transacciones por categoría o descripción.
* 📈 **Resumen de Saldos:** Cálculo automático de ingresos, egresos y saldo neto con indicadores visuales de color.
* 📤 **Exportación a CSV:** Exporta tus reportes financieros fácilmente para abrirlos en Excel o Google Sheets.

---

## 🛠️ Tecnologías y Entorno

* **Lenguaje:** C# (.NET Framework 4.x / WinForms)
* **Almacenamiento:** JSON local (`JavaScriptSerializer`)
* **Compilación:** Automatizada mediante script por lotes (`.bat`)

---

## 🚀 Cómo compilar y ejecutar localmente

Si deseas clonar o descargar el código y compilarlo por ti mismo en Windows:

1. Clona el repositorio o descarga el archivo `finopticon.cs`.
2. Coloca el código en tu carpeta de desarrollo local (por ejemplo, `C:\Users\TuUsuario\Desktop\Dev`).
3. Ejecuta tu script de compilación o utiliza el comando nativo del compilador de C#:
   ```cmd
   csc.exe /target:winexe /r:System.Windows.Forms.dll,System.Drawing.dll,System.Web.Extensions.dll finopticon.cs
