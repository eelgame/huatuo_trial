using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartupMono : MonoBehaviour
{
    public InputField InputField;

    public Button Button;
    
    // Start is called before the first frame update
    void Start()
    {
        BetterStreamingAssets.Initialize();

        // if (Application.isBatchMode)
        // {
        //     var bytes = File.ReadAllBytes(@"C:\Users\18611\Desktop\huatuo_trial\Temp\HuaTuo\build\StandaloneWindows64\2048.dll");
        //     Assembly.Load(bytes);
        //     var assetBundle = BetterStreamingAssets.LoadAssetBundle("scene");
        //     var allScenePaths = assetBundle.GetAllScenePaths();
        //
        //     SceneManager.LoadScene(allScenePaths[0]);
        // }
    }

    public void Enter()
    {
        var bytes = File.ReadAllBytes(@"C:\Users\18611\Desktop\huatuo_trial\Temp\HuaTuo\build\StandaloneWindows64\2048.dll");
        Assembly.Load(bytes);
        var assetBundle = BetterStreamingAssets.LoadAssetBundle("scene");
        var allScenePaths = assetBundle.GetAllScenePaths();

        SceneManager.LoadScene(allScenePaths[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
