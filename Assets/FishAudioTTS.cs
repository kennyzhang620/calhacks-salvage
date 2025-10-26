using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class FishAudioTTS : MonoBehaviour
{
    private const string API_URL = "https://api.fish.audio/v1/tts";
    private const string API_KEY;
    private AudioSource audioSource;

    private async void Start()
    {
        audioSource = GetComponent<AudioSource>();

      //  string text = "Hello! Welcome to Fish Audio. This is my first AI-generated voice.";
       // await GenerateAndPlayTTSAsync(text, "mp3");
    }

    public async Task GenerateAndPlayTTSAsync(string text, string format)
    {
        // Prepare JSON payload
        var requestBody = new { text = text, format = format };
        string output = new string(text.Where(c => !char.IsControl(c)).ToArray());
        string json = "{\"text\":\"" + output + "\",\"format\":\"" + format + "\"}";
        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("model", "s1");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ TTS request succeeded!");

                byte[] audioData = request.downloadHandler.data;
                if (audioData == null || audioData.Length < 44)
                {
                    Debug.LogError("❌ Invalid WAV data received from server.");
                    return;
                }

                // Decode WAV to AudioClip
                AudioClip clip = WavUtility.ToAudioClip(audioData, "FishTTSClip");
                if (clip == null)
                {
                    Debug.LogError("❌ Failed to decode WAV data.");
                    return;
                }

                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log("🎵 Playing TTS audio!");
            }
            else
            {
                Debug.LogError($"❌ TTS request failed: {request.error}\n{request.downloadHandler.text}");
            }
        }
    }

private void Update()
    {
        if (GameManager.SaySpeak.Length > 0)
        {
            GenerateAndPlayTTSAsync(GameManager.SaySpeak, "wav");
            GameManager.SaySpeak = "";
        }
    }
}
