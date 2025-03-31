using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class VoiceToImage : MonoBehaviour
{
    public Image uiImage; // Assign this in the Unity Inspector
    private string apiURL = "http://192.168.0.102:7860/sdapi/v1/txt2img";
    public TMP_Text promptText;
    public TMP_Text debugText;
    
    void Start()
    {
        // Initialize Meta XR Voice SDK (Assume SDK integration is done)
    }

    public void SendPrompt()
    {
        OnVoiceInputReceived(promptText.text);
    }

    private void OnVoiceInputReceived(string prompt)
    {
        debugText.text ="Voice Input: " + prompt;
        StartCoroutine(GenerateImage(prompt));
    }

    IEnumerator GenerateImage(string prompt)
    {
        string jsonData = "{\"prompt\": \"" + prompt + "\", \"steps\": 20}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(apiURL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            request.certificateHandler = new AcceptAllCertificates(); // Allow HTTP

            debugText.text = "\nSending request to: " + apiURL;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                debugText.text = "\nResponse: " + responseJson;  // ✅ Log the response

                StableDiffusionResponse response = JsonUtility.FromJson<StableDiffusionResponse>(responseJson);
                if (response != null && response.images.Length > 0)
                {
                    StartCoroutine(LoadImage(response.images[0]));
                }
                else
                {
                    debugText.text = "\nNo images in response.";
                }
            }
            else
            {
                debugText.text = "\nError: " + request.error;
            }
        }
    }

    IEnumerator LoadImage(string base64Image)
    {
        byte[] imageBytes = System.Convert.FromBase64String(base64Image);
        Texture2D texture = new Texture2D(512, 512);
        texture.LoadImage(imageBytes);

        Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        uiImage.sprite = newSprite;
        debugText.text = "\nSprite loaded: " + newSprite.name;

        yield return null;
    }

    [System.Serializable]
    private class StableDiffusionResponse
    {
        public string[] images;
    }
    
    private class AcceptAllCertificates : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }
}


