using System.IO;
using System.Collections.Generic;
using UnityEditor;
using Newtonsoft.Json;

namespace uprt.editor
{
    /// <summary>
    /// UPRT Unity Asset Sync를 위한 클래스
    /// </summary>
    public class UnityAssetGUID
    {
        /// <summary>
        /// 파일 경로
        /// </summary>
        public string Path;
        /// <summary>
        /// 에셋 이름
        /// </summary>
        public string Name;
        /// <summary>
        /// GUID
        /// </summary>
        public string GUID;
        /// <summary>
        /// File ID
        /// </summary>
        public long FileID;
    }

    public partial class UPRTPackageManager
    {
        /// <summary>
        /// Unity Asset의 GUID를 수집하는 함수
        /// </summary>
        public static void GetGUIDs()
        {
            string outputPath = string.Empty;
            string packageTitle = string.Empty;
            string[] arguments = System.Environment.GetCommandLineArgs();
            bool isGetOutputPath = false;

            for (int idx = 0; idx < arguments.Length; idx++)
            {
                if (arguments[idx] == "-output")
                {
                    outputPath = arguments[idx + 1];
                    isGetOutputPath = true;
                    continue;
                }

                if (isGetOutputPath && arguments[idx] == "-packageTitle")
                {
                    packageTitle = arguments[idx + 1];
                    break;
                }
            }

            if (string.IsNullOrEmpty(outputPath) || string.IsNullOrEmpty(packageTitle))
                return;

            List<UnityAssetGUID> unityAssetGUIDs = new List<UnityAssetGUID>();

            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (path.IndexOf($"Packages/{packageTitle}") != 0 && path.IndexOf("Assets") != 0)
                    continue;
                if (Path.GetExtension(path).ToLower() != ".cs" && Path.GetExtension(path).ToLower() != ".dll")
                    continue;

                foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    string assetGuid;
                    long assetLocalID;

                    if (obj == null)
                        continue;

                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out assetGuid, out assetLocalID))
                    {
                        if (obj is MonoScript)
                        {
                            var classType = (obj as MonoScript)?.GetClass();
                            if (classType != null)
                            {
                                unityAssetGUIDs.Add(new UnityAssetGUID
                                {
                                    Path = path,
                                    Name = classType.FullName,
                                    GUID = assetGuid,
                                    FileID = assetLocalID
                                });
                            }
                        }
                    }
                }
            }
            string json = JsonConvert.SerializeObject(unityAssetGUIDs, Formatting.Indented);
            File.WriteAllText(outputPath, json);
        }
    }
}
