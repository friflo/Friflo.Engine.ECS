
#if UNITY_5_3_OR_NEWER

using System.IO;
using UnityEditor;
using UnityEngine;
using static NUnit.Framework.Assert;

namespace Tests.Utils
{
    public class UnityUtils : ScriptableObject
    {
        public static string GetProjectFolder() {
            ScriptableObject unityUtils = ScriptableObject.CreateInstance<UnityUtils>();
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(unityUtils));
            // Assets/Friflo.Json.Fliox/Engine/Tests/Utils/UnityUtils.cs
            string expectedPath = "Tests/Utils/UnityUtils.cs";
            if (!scriptPath.EndsWith(expectedPath))
                Fail("Expect UnityUtils located in: " + expectedPath);
            string projectBase = Path.GetDirectoryName(scriptPath) + "/../";
            string baseDir = Directory.GetCurrentDirectory() + "/" + projectBase;
            return baseDir;
        }
    }
}

#endif
