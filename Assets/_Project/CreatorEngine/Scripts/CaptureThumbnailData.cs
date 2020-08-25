using System.IO;
using UnityEngine;
using System;

public class CaptureThumbnailData : MonoBehaviour
{
    byte[] bytes;
    string name = "";
    public byte[] Capture(int width, int height, string name = "")
    {
        this.name = Guid.NewGuid().ToString();
        if(name.Length > 0)
            this.name = name;
        Texture2D image = GetTexture();

        // Read screen contents into the texture
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        bytes = image.EncodeToPNG();
        Destroy(image);
        
        // Alt SaveToForm or pass Binary to form
        SaveToDisk(bytes);
        return bytes;
    }

    public Texture2D GetTexture()
    {
        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        return tex;
    }

    public void SaveToForm(byte[] bytes)
    {
        // rework to save via API

        //var form = new WWWForm ();
        //form.AddField ( "action", "Upload Image" );
        //form.AddBinaryData ( "fileUpload", bytes, "screenShot.png", "image/png" );
    }

    public void SaveToDisk(byte[] bytes)
    {
        File.WriteAllBytes(Application.dataPath + "/Backgrounds/" + name + ".png", bytes);
    }
}
