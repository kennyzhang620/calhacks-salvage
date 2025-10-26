using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class OpenAIClient
{
    private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

    // ⚠️ Store your API key securely (e.g. environment variable, Unity Cloud secret, or external config)
    private static string apiKey;
    // 🔹 Call this from anywhere: string reply = await OpenAIClient.GetChatCompletion("Hello there!");
    public static async Task<string> GetChatCompletion(string userMessage, string model = "gpt-4o-mini")
    {

        if (!GameManager.UseCredit(0)) return null;

        // Prepare request JSON
        ChatRequest requestBody = new ChatRequest
        {
            model = model,
            messages = new Message[]
            {
                new Message { role = "user", content = userMessage }
            }
        };

        string json = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            // Attach handlers
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();

            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            // Send async
            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield(); // keep async-friendly, no blocking

            // Check response
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                ChatResponse response = JsonUtility.FromJson<ChatResponse>(responseText);

                // return assistant message
                Debug.Log("nay");
                return response.choices[0].message.content.Trim();
            }
            else
            {
                Debug.Log("ah");
                Debug.Log($"OpenAI Error: {request.responseCode}\n{request.error}\n{request.downloadHandler.text}");
                return null;
            }
        }
    }

    // --- Data models ---
    [System.Serializable] public class Message { public string role; public string content; }
    [System.Serializable] public class ChatRequest { public string model; public Message[] messages; }
    [System.Serializable] public class ChatResponse { public Choice[] choices; }
    [System.Serializable] public class Choice { public Message message; }
}
