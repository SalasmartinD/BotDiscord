using System.Text;
using System.Text.Json;

public class GeminiService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    // DEFINIMOS LA PERSONALIDAD AQUÍ
    private const string PERSONALIDAD = @"
        Eres Astolfo, el Rider de los Doce Paladines de Carlomagno (del anime Fate/Apocrypha).
        Tu personalidad es: Enérgica, alegre, optimista, un poco distraída y muy leal.
        Hablas de forma casual, divertida y usas exclamaciones.
        A veces te refieres a quien te habla como 'Master' (Maestro).
        No eres un asistente de IA aburrido, eres un Paladín heroico y lindo.
        Responde siempre en español.
        IMPORTANTE: Tus respuestas deben ser breves (menos de 1800 caracteres) para que quepan en Discord.
        Sé conciso, como en un chat rápido.";

    public GeminiService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task<string> ResponderComoAstolfo(string historialChat, string nombreUsuario, string mensajeUsuario)
    {
        // Uso el modelo rápido
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemma-3-12b-it:generateContent?key={_apiKey}";
        
        // Prompt mejorado con contexto
        var promptFinal = $@"
        {PERSONALIDAD}

        CONTEXTO DEL CHAT (Lo que hablaron antes):
        {historialChat}

        MENSAJE ACTUAL:
        {nombreUsuario}: {mensajeUsuario}

        INSTRUCCIÓN:
        Responde al mensaje actual teniendo en cuenta el contexto anterior si es relevante. 
        Si el contexto no tiene nada que ver, ignóralo.
        Recuerda ser breve.";

        var payload = new
        {
            contents = new[] { new { parts = new[] { new { text = promptFinal } } } }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try 
        {
            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) return $"❌ Error técnico ({response.StatusCode})";
            
            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            // Validaciones de seguridad
            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                return "🤖 ...";

            var primerCandidato = candidates[0];
            if (!primerCandidato.TryGetProperty("content", out var contentElement))
                return "🙈 (Astolfo se tapa los ojos por seguridad)";

            return contentElement.GetProperty("parts")[0].GetProperty("text").GetString() ?? "🤔";
        }
        catch (Exception ex)
        {
            return $"😵 Error: {ex.Message}";
        }
    }
}