﻿#if UNITY_2020_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System;
using UnityEditor.UnityLinker;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor.Il2Cpp;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace HuaTuo
{
    public class HuaTuo_BuildProcessor_2020_1_OR_NEWER : IPreprocessBuildWithReport, IIl2CppProcessor
#if UNITY_ANDROID
        , IPostGenerateGradleAndroidProject
#else
        , IPostprocessBuildWithReport
#endif
        , IProcessSceneWithReport, IFilterBuildAssemblies, IPostBuildPlayerScriptDLLs, IUnityLinkerProcessor
    {
        
        public void OnBeforeConvertRun(BuildReport report, Il2CppBuildPipelineData data)
        {
            try
            {
                foreach (var name in s_allHotUpdateDllNames)
                {
                    File.Move("Temp/StagingArea/Data/Managed/" + name, "Temp/StagingArea/Data/Managed/_cheat_" + name);
                }
            }
            catch (Exception e)
            {
                throw new BuildFailedException(e);
            }
        }
        
        /// <summary>
        /// 需要在Prefab上挂脚本的热更dll名称列表，不需要挂到Prefab上的脚本可以不放在这里
        /// 但放在这里的dll即使勾选了 AnyPlatform 也会在打包过程中被排除
        /// 
        /// 另外请务必注意！： 需要挂脚本的dll的名字最好别改，因为这个列表无法热更（上线后删除或添加某些非挂脚本dll没问题）
        /// </summary>
        static List<string> s_monoHotUpdateDllNames = new List<string>()
        {
            "2048.dll",
        };


        /// <summary>
        /// 所有热更新dll列表
        /// </summary>
        public static List<string> s_allHotUpdateDllNames = s_monoHotUpdateDllNames.Concat(new List<string>
        {
            // 这里放除了s_monoHotUpdateDllNames以外的脚本不需要挂到资源上的dll列表
        }).ToList();

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {

        }

        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            // 将热更dll从打包列表中移除
            return assemblies;
        }


        [Serializable]
        public class ScriptingAssemblies
        {
            public List<string> names;
            public List<int> types;
        }

#if UNITY_ANDROID
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // 由于 Android 平台在 OnPostprocessBuild 调用时已经生成完 apk 文件，因此需要提前调用
            AddBackHotFixAssembliesToJson(null, path);
        }
#else
        public void OnPostprocessBuild(BuildReport report)
        {
            try
            {
                AddBackHotFixAssembliesToJson(report, report.summary.outputPath);
            
                string[] files = Directory.GetFiles(Path.GetDirectoryName(report.summary.outputPath), "CMakeLists.txt", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var text = File.ReadAllText(file);
                    text = "set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/../../)\n" + text;
                    text = Regex.Replace(text, @"\/Yupch\-(c|cpp)\.(h|hpp) \/Fp\\\"".*\\\\[a-zA-Z0-9]+\.pch\\\""", "");
                    
                    File.WriteAllText(file, text);
                }
            }
            catch (Exception e)
            {
                throw new BuildFailedException(e);
            }
        }
#endif

        private void AddBackHotFixAssembliesToJson(BuildReport report, string path)
        {
            /*
             * ScriptingAssemblies.json 文件中记录了所有的dll名称，此列表在游戏启动时自动加载，
             * 不在此列表中的dll在资源反序列化时无法被找到其类型
             * 因此 OnFilterAssemblies 中移除的条目需要再加回来
             */
            string[] jsonFiles = Directory.GetFiles(Path.GetDirectoryName(path), "ScriptingAssemblies.json", SearchOption.AllDirectories);

            if (jsonFiles.Length == 0)
            {
                Debug.LogError("can not find file ScriptingAssemblies.json");
                return;
            }

            foreach (string file in jsonFiles)
            {
                string content = File.ReadAllText(file);
                ScriptingAssemblies scriptingAssemblies = JsonUtility.FromJson<ScriptingAssemblies>(content);
                foreach (string name in s_monoHotUpdateDllNames)
                {
                    if(!scriptingAssemblies.names.Contains(name))
                    {
                        scriptingAssemblies.names.Add(name);
                        scriptingAssemblies.types.Add(16); // user dll type
                    }
                }
                content = JsonUtility.ToJson(scriptingAssemblies);

                File.WriteAllText(file, content);
            }
        }


        public void OnProcessScene(Scene scene, BuildReport report)
        {

        }

        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {

        }

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            return String.Empty;
        }

        public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

        public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }


#if UNITY_IOS
    // hook UnityEditor.BuildCompletionEventsHandler.ReportPostBuildCompletionInfo() ? 因为没有 mac 打包平台因此不清楚
#endif
    }

}
#endif