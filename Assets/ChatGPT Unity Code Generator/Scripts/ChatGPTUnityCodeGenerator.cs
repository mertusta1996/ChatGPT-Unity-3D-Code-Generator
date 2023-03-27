#nullable enable
using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using File = System.IO.File;

public class ChatGPTUnityCodeGenerator : Singleton<ChatGPTUnityCodeGenerator>
{
    [Header("OpenAI ChatGPT API key")]
    public string apiKey = "--- here add your OpenAI ChatGPT API key ---";
    
    [Header("OpenAI ChatGPT model name")]
    public string modelName = "gpt-3.5-turbo";

    [Header("New Unity 3D code's name")]
    public string newScriptName = "NewScript";
    
    [Header("What is the new Unity3D code's instruction ?")]
    [SerializeField, TextArea] public string message = "--- here add your Unity3D code's instruction ---";

    private ChatGPTConnection? connection;
    
    [ContextMenu(nameof(GenerateUnity3DCode))]
    public async void GenerateUnity3DCode()
    {
        Assert.IsNotNull(apiKey);
        connection = new ChatGPTConnection(apiKey, modelName);

        if (connection == null)
        {
            Debug.LogError($"No valid connection.");
            return;
        }

        if (string.IsNullOrEmpty(message))
        {
            Debug.LogError($"Add a valid Unity3D code instruction.");
            return;
        }

        ResponseBody result;
        try
        {
            var unityAttachedMessage = "create a c# code that works in Unity 3D, " + message;
            // Create message by ChatGPT chat completion API.
            result = await connection
                .CreateMessageAsync(unityAttachedMessage, this.GetCancellationTokenOnDestroy());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return;
        }

        Debug.Log($"Script:\n{result.ResultMessage}");
        GenerateNewUnityScript(result.ResultMessage);
    }
    
    private void GenerateNewUnityScript(string scriptContent)
    {
        if (!scriptContent.Contains("using UnityEngine;"))
        {
            return;
        }
        
        var str = scriptContent.Split("using UnityEngine;")[1];
        string[] words = str.Split(' ');
        bool isSuitable = words[0].ToLower() == "using UnityEngine;".ToLower();

        if (!isSuitable)
            str = "using UnityEngine;" + str;
        
        var newScript = str.Remove(str.LastIndexOf('}')+1);
        var gptClassName = newScript.Split("public class ")[1];
        var gptWords = gptClassName.Split(' ');
        newScript = newScript.Replace("public class " + gptWords[0], "public class " + newScriptName);

        var path = "Assets/ChatGPT Unity Code Generator/#Generated Unity Code/" + newScriptName + ".cs";
        File.WriteAllText(path, newScript);
        AssetDatabase.ImportAsset(path);
    }

    [ContextMenu(nameof(AddGeneratedCodeToScene))]
    public void AddGeneratedCodeToScene()
    {
        string typeName = newScriptName + ", Assembly-CSharp";
        var type = Type.GetType(typeName);
        
        var go = new GameObject();
        go.name = newScriptName;
        go.AddComponent(type);
    }
}