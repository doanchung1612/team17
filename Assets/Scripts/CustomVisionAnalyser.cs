using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomVisionAnalyser : MonoBehaviour
{
    /// <summary>
    /// Unique instance of this class
    /// </summary>
    public static CustomVisionAnalyser Instance;

    /// <summary>
    /// Insert your prediction key here
    /// </summary>
    private string predictionKey = "1c2e1896b9184dcdb86ca9363868527e";

    /// <summary>
    /// Insert your prediction endpoint here
    /// </summary>
    //private string predictionEndpoint = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/";
    private string predictionEndpoint = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/2b881b45-2b5a-4516-839e-810604bd7232/image?iterationId=a7454f4c-f089-4d2b-8083-6b2dc289391b";
    
    /// <summary>
    /// Bite array of the image to submit for analysis
    /// </summary>
    [HideInInspector] public byte[] imageBytes;

    /// <summary>
    /// Initializes this class
    /// </summary>
    private void Awake()
    {
        // Allows this instance to behave like a singleton
        Instance = this;
        //m_Image = GetComponent<Image>();
    }

    /// <summary>
    /// Call the Computer Vision Service to submit the image.
    /// </summary>
    public IEnumerator AnalyseLastImageCaptured(string imagePath)
    {
        Debug.Log("Analyzing...");

        WWWForm webForm = new WWWForm();

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(predictionEndpoint, webForm))
        {
            // Gets a byte array out of the saved image
            imageBytes = GetImageAsByteArray(imagePath);

            unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
            unityWebRequest.SetRequestHeader("Prediction-Key", predictionKey);

            // The upload handler will help uploading the byte array with the request
            unityWebRequest.uploadHandler = new UploadHandlerRaw(imageBytes);
            unityWebRequest.uploadHandler.contentType = "application/octet-stream";

            // The download handler will help receiving the analysis from Azure
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            // Send the request
            yield return unityWebRequest.SendWebRequest();

            string jsonResponse = unityWebRequest.downloadHandler.text;

            Debug.Log("response: " + jsonResponse);

            // Create a texture. Texture size does not matter, since
            // LoadImage will replace with the incoming image size.
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(imageBytes);
            SceneOrganiser.Instance.quadRenderer.material.SetTexture("_MainTex", tex);

            // The response will be in JSON format, therefore it needs to be deserialized
            AnalysisRootObject analysisRootObject = new AnalysisRootObject();
            analysisRootObject = JsonConvert.DeserializeObject<AnalysisRootObject>(jsonResponse);

            SceneOrganiser.Instance.FinaliseLabel(analysisRootObject);
        }
    }

    //   /// <summary>
    ///// Call the Computer Vision Service to submit the image.
    ///// </summary>
    //public IEnumerator AnalyseLastImageCaptured(string imagePath)
    //   {
    //       WWWForm webForm = new WWWForm();
    //       using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(visionAnalysisEndpoint, webForm))
    //       {
    //           // gets a byte array out of the saved image
    //           imageBytes = GetImageAsByteArray(imagePath);
    //           Debug.Log(imageBytes);
    //           unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
    //           unityWebRequest.SetRequestHeader(ocpApimSubscriptionKeyHeader, authorizationKey);

    //           // the download handler will help receiving the analysis from Azure
    //           unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

    //           // the upload handler will help uploading the byte array with the request
    //           unityWebRequest.uploadHandler = new UploadHandlerRaw(imageBytes);
    //           unityWebRequest.uploadHandler.contentType = "application/octet-stream";

    //           yield return unityWebRequest.SendWebRequest();


    //           long responseCode = unityWebRequest.responseCode;
    //           Debug.Log(unityWebRequest.responseCode);


    //           try
    //           {
    //               string jsonResponse = null;
    //               jsonResponse = unityWebRequest.downloadHandler.text;
    //               Debug.Log("response: " + jsonResponse);

    //               //####################
    //               //Create a texture.Texture size does not matter, since
    //               // LoadImage will replace with the incoming image size.
    //               Texture2D tex = new Texture2D(1, 1);
    //               tex.LoadImage(imageBytes);
    //               SceneOrganiser.Instance.quadRenderer.material.SetTexture("_MainTex", tex);

    //               // The response will be in JSON format, therefore it needs to be deserialized
    //               AnalysisRootObject analysisRootObject = new AnalysisRootObject();
    //               analysisRootObject = JsonConvert.DeserializeObject<AnalysisRootObject>(jsonResponse);

    //               SceneOrganiser.Instance.FinaliseLabel(analysisRootObject);

    //               //####################
    //               // The response will be in Json format
    //               // therefore it needs to be deserialized into the classes AnalysedObject and TagData
    //               //AnalysedObject analysedObject = new AnalysedObject();
    //               //analysedObject = JsonUtility.FromJson<AnalysedObject>(jsonResponse);
    //               //if (analysedObject.tags == null)
    //               //{
    //               //    Debug.Log("analysedObject.tagData is null");
    //               //}
    //               //else
    //               //{
    //               //    Dictionary<string, float> tagsDictionary = new Dictionary<string, float>();

    //               //    foreach (TagData td in analysedObject.tags)
    //               //    {
    //               //        TagData tag = td as TagData;
    //               //        tagsDictionary.Add(tag.name, tag.confidence);
    //               //    }

    //               //    //ResultsLabel.instance.SetTagsToLastLabel(tagsDictionary);

    //               //}
    //           }
    //           catch (Exception exception)
    //           {
    //               Debug.Log("Json exception.Message: " + exception.Message);
    //           }

    //           yield return null;
    //       }
    //   }

    /// <summary>
    /// Returns the contents of the specified image file as a byte array.
    /// </summary>
    static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);

        BinaryReader binaryReader = new BinaryReader(fileStream);

        return binaryReader.ReadBytes((int)fileStream.Length);
    }
}
