# 🤖 AstolfoBot - AI Powered Discord Assistant

**AstolfoBot** es un bot de Discord desarrollado en **C# (.NET 10)** que integra Inteligencia Artificial Generativa (Google Gemini/Gemma) para ofrecer conversaciones naturales, contextuales y divertidas. 

A diferencia de los bots tradicionales basados en comandos rígidos, este proyecto implementa un "cerebro" capaz de recordar el contexto de la conversación, adoptar una personalidad específica y (próximamente) ejecutar tareas de administración de sistemas mediante lenguaje natural.

## 🚀 Características Principales

### 🧠 Inteligencia Artificial Integrada
- **Conversación Natural:** Utiliza la API de Google Generative AI (Modelos `Gemini 1.5` o `Gemma 3`).
- **Memoria a Corto Plazo:** El bot lee y procesa los últimos mensajes del canal para entender el contexto y responder coherentemente a hilos de conversación.
- **Personalidad Personalizable:** Configurado mediante *System Instructions* para emular a "Astolfo" (Fate/Apocrypha), pero adaptable a cualquier rol.
- **Manejo de Respuestas Largas:** Sistema de *Chunking* inteligente que divide respuestas extensas de la IA en fragmentos de 1900 caracteres para cumplir con los límites de Discord sin cortar oraciones.

### 🛠️ Administración de Sistemas (System Control)
- **Control de Procesos:** Capacidad para verificar, iniciar y detener procesos del servidor (como `playit.exe` o servidores de juegos) directamente desde el código.
- **Árbol de Procesos:** Implementación de `Kill(true)` para asegurar que al detener un servicio, se cierren también las consolas y subprocesos asociados.

### 🛡️ Robustez Técnica
- **Manejo de Rate Limits:** Lógica para detectar errores `429 Too Many Requests` y gestionar cuotas de API.
- **Activación Flexible:** Responde a menciones (@Bot), respuestas directas (Replies) o palabras clave en el mensaje.

## 🛠️ Tecnologías Usadas

- **Lenguaje:** C# (C Sharp)
- **Framework:** .NET 10.0
- **Librerías:** - `Discord.Net` (Interacción con la API de Discord)
  - `System.Diagnostics.Process` (Control del Sistema Operativo)
  - `HttpClient` (Consumo de API REST de Google)

## 📋 Requisitos Previos

Para ejecutar este bot necesitas:
1.  **.NET 10 SDK** instalado.
2.  Una cuenta de desarrollador en [Discord Developer Portal](https://discord.com/developers/applications) para obtener el Token.
3.  Una API Key de [Google AI Studio](https://aistudio.google.com/) (Gratuita).

## ⚙️ Configuración

1.  Clona el repositorio:
    ```bash
    git clone https://github.com/SalasmartinD/BotDiscord.git
    ```
2.  Crea un archivo `appsettings.json` en la raíz del proyecto (o usa User Secrets) con la siguiente estructura:
    ```json
    {
      "Token": "TU_TOKEN_DE_DISCORD_AQUI",
      "GoogleApiKey": "TU_API_KEY_DE_GOOGLE_AQUI",
      "PcConfig": {
        // Configuraciones adicionales si las tienes
      }
    }
    ```
3.  **Selección del Modelo de IA:**
    En `GeminiService.cs`, puedes configurar el modelo a utilizar. Se recomienda usar modelos con límites altos de RPD (Requests Per Day) como `gemma-3-12b-it` o `gemini-1.5-flash`.

## 💻 Uso

Una vez que el bot está corriendo, puedes interactuar con él de forma natural:

- **Charla Casual:**
  > *Usuario:* "Astolfo, ¿qué opinas de este servidor?"
  > *Bot:* "¡Es genial Master! Aunque le falta un poco de caos, ¡jaja! 🎶"

- **Contexto:**
  > *Usuario:* "Me voy a dormir."
  > *Usuario:* "Astolfo, diles buenas noches a todos."
  > *Bot:* "¡Descansen bien! @everyone dulces sueños ✨"

## 🚧 Próximos Pasos (Roadmap)

- [ ] **Function Calling con IA:** Permitir que la IA decida autónomamente cuándo ejecutar comandos de sistema (ej: "Prende el server") analizando la intención del usuario.
- [ ] **Base de Datos:** Persistencia de configuraciones por servidor.
- [ ] **Comandos Slash
