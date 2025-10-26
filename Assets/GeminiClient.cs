using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class GeminiClient
{
    // ⚠️ Replace with your Gemini API key (secure it in production)
    private static string apiKey;
    private static string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent?key={1}";

    // 🔹 Call this from anywhere: string reply = await GeminiClient.GenerateText("Write a Unity tutorial intro.");
    public static async Task<string> GetChatCompletion(string prompt, string model = "gemini-2.0-flash")
    {
        if (!GameManager.UseCredit(1)) return null;
        string url = string.Format(apiUrl, model, apiKey);

        // Build request JSON
        GeminiRequest requestBody = new GeminiRequest
        {
            contents = new Content[]
            {
                new Content
                {
                    parts = new Part[]
                    {
                        new Part { text = prompt }
                    }
                }
            }
        };

        string json = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(responseText);

                // Extract the text (Gemini sometimes nests deeper)
                if (response.candidates != null && response.candidates.Length > 0)
                {
                    return response.candidates[0].content.parts[0].text.Trim();
                }
                else
                {
                    Debug.LogWarning("Gemini response had no candidates:\n" + responseText);
                    return null;
                }
            }
            else
            {
                Debug.LogError($"Gemini API Error: {request.responseCode}\n{request.error}\n{request.downloadHandler.text}");
                return null;
            }
        }
    }

    // --- Data models ---
    [System.Serializable] public class Part { public string text; }
    [System.Serializable] public class Content { public Part[] parts; }
    [System.Serializable] public class GeminiRequest { public Content[] contents; }

    [System.Serializable] public class Candidate { public Content content; }
    [System.Serializable] public class GeminiResponse { public Candidate[] candidates; }
}
