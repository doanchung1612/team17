using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class VisionManager : MonoBehaviour
{

	Image m_Image;
	//Set this in the Inspector
	//public Sprite m_Sprite;

	public static VisionManager instance;

	// you must insert your service key here!    
	private string authorizationKey = "22e13df61b6f4a71971b6762cdab7e92";    
	// private string authorizationKey = "22e13df61b6f4a71971b6762cdab7e92"; 
	private const string ocpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";
	private string visionAnalysisEndpoint = "https://southeastasia.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags";   // This is where you need to update your endpoint, if you set your location to something other than west-us.

	internal byte[] imageBytes;
	internal string imagePath;


	private void Awake()
	{
		// allows this instance to behave like a singleton
		instance = this;
		m_Image = GetComponent<Image>();
	}
 


	/// <summary>
	/// Call the Computer Vision Service to submit the image.
	/// </summary>
	public IEnumerator AnalyseLastImageCaptured()
	{
        Debug.Log("Analyzing...");
        WWWForm webForm = new WWWForm();
		using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(visionAnalysisEndpoint, webForm))
		{
			// gets a byte array out of the saved image
			imageBytes = GetImageAsByteArray(imagePath);
			Debug.Log(imageBytes);
			unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
			unityWebRequest.SetRequestHeader(ocpApimSubscriptionKeyHeader, authorizationKey);

			// the download handler will help receiving the analysis from Azure
			unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

			// the upload handler will help uploading the byte array with the request
			unityWebRequest.uploadHandler = new UploadHandlerRaw(imageBytes);
			unityWebRequest.uploadHandler.contentType = "application/octet-stream";

			yield return unityWebRequest.SendWebRequest();

			long responseCode = unityWebRequest.responseCode;    
			Debug.Log(unityWebRequest.responseCode) ;

			try
			{
				string jsonResponse = null;
				jsonResponse = unityWebRequest.downloadHandler.text;

                // Create a texture. Texture size does not matter, since
                // LoadImage will replace with the incoming image size.
                //Texture2D tex = new Texture2D(1, 1);
                //tex.LoadImage(imageBytes);
                //ResultsLabel.instance.quadRenderer.material.SetTexture("_MainTex", tex);

                //AnalysisRootObject analysisRootObject = new AnalysisRootObject();
                //analysisRootObject = JsonConvert.DeserializeObject<AnalysisRootObject>(jsonResponse);

                //ResultsLabel.instance.FinaliseLabel(analysisRootObject);


                //            // The response will be in Json format
                //            // therefore it needs to be deserialized into the classes AnalysedObject and TagData
                AnalysisRootObject analysedObject = new AnalysisRootObject();
                analysedObject = JsonUtility.FromJson<AnalysisRootObject>(jsonResponse);

                if (analysedObject.tags == null)
                {
                    Debug.Log("analysedObject.tagData is null");
                }
                else
                {
                    Dictionary<string, float> tagsDictionary = new Dictionary<string, float>();

                    foreach (TagData td in analysedObject.tags)
                    {
                        TagData tag = td as TagData;
                        tagsDictionary.Add(tag.name, tag.confidence);
                    }
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(imageBytes);
                    ResultsLabel.instance.quadRenderer.material.SetTexture("_MainTex", tex);
                    ResultsLabel.instance.SetTagsToLastLabel(tagsDictionary);
                }
            }
			catch (Exception exception)
			{
				Debug.Log("Json exception.Message: " + exception.Message);
			}

			yield return null;
		}
	}

	/// <summary>
	/// Returns the contents of the specified file as a byte array.
	/// </summary>
	private static byte[] GetImageAsByteArray(string imageFilePath)
	{
		FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		return binaryReader.ReadBytes((int)fileStream.Length);
	}
}
