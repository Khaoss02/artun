using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Linq;

// Clases de utilidad para la API (Mantenidas exactas)
public class ChatEntry { public string role { get; set; } public string content { get; set; } }
public class ChatApiResponse { public List<ChatChoice> choices { get; set; } }
public class ChatChoice { public ChatEntry message { get; set; } }

public sealed class FishBrain : Component
{
    [Property] public string ApiKey { get; set; } = "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";

    [Property] public FishResource Data { get; set; }

    public string Personalidad { get; private set; }
    public string FishName { get; private set; }

    private List<ChatEntry> Memoria = new List<ChatEntry>();

    protected override void OnStart()
    {
        if ( Data != null )
        {
            CargarPez( Data );
        }
        else
        {
            FishName = "Pez Genérico";
            Personalidad = "Eres un pez común. Estás confundido por estar fuera del agua.";
            Log.Warning( $"FishBrain en {GameObject.Name} no tiene un FishResource asignado!" );
        }
    }

    /// <summary>
    /// Esta es la función que llama el inventario/chat para cambiar el pez
    /// </summary>
    public void CargarPez( FishResource nuevoPez )
    {
        if ( nuevoPez == null ) return;

        Data = nuevoPez;
        FishName = Data.Name;
        Personalidad = $"{Data.Personality} Speak directly. No emojis. No parentheses for actions.";
        
        // Limpiamos memoria para que el nuevo pez no sepa lo que dijo el anterior
        Memoria.Clear();
        Memoria.Add( new ChatEntry { role = "system", content = Personalidad } );
        
        Log.Info( $"[FishBrain] Identidad actualizada a: {FishName}" );
    }

    public async Task<string> HablarConIA( string mensajeUsuario )
    {
        // Aseguramos que el system prompt esté presente
        if ( Memoria.Count == 0 )
        {
            Memoria.Add( new ChatEntry { role = "system", content = Personalidad } );
        }

        Memoria.Add( new ChatEntry { role = "user", content = mensajeUsuario } );

        if ( Memoria.Count > 11 ) Memoria.RemoveAt( 1 );

        try
        {
            // ESTRUCTURA DE PAYLOAD ORIGINAL
            var payload = new
            {
                model = "nvidia/nemotron-3-nano-30b-a3b:free",
                messages = Memoria
            };

            // ENVÍO DE DATOS ORIGINAL (StringContent + RequestJsonAsync)
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Http.RequestJsonAsync<ChatApiResponse>(
                "https://openrouter.ai/api/v1/chat/completions",
                "POST",
                content,
                new Dictionary<string, string> { { "Authorization", $"Bearer {ApiKey}" } }
            );

            if ( response?.choices != null && response.choices.Count > 0 )
            {
                string respuestaTexto = response.choices[0].message.content;
                Memoria.Add( new ChatEntry { role = "assistant", content = respuestaTexto } );
                return respuestaTexto;
            }
        }
        catch ( System.Exception e )
        {
            Log.Error( $"Fish AI Error: {e.Message}" );
        }

        return "Glub... (Mi cerebro de pez se ha quedado en blanco)";
    }
}
