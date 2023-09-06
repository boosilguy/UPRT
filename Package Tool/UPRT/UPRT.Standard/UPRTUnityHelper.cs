using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using UPRT.Standard.Models;
using UPRT.Standard.Utility;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace UPRT.Standard
{
    public class UPRTUnityHelper
    {
        static readonly string[] GuidSyncExtensions =
        {
            "*.unity",
            "*.asset",
            "*.prefab",
            "*.mat",
            "*.anim"
        };

        const int SUCCESS_CODE = 0;
        const int FAIL_CODE = 1;

        const string SYNC_GUID_TARGET_ARGS = "-output";
        const string PACKAGES_DIR = "Packages";
        const string SCRIPT_ASSEMBLIES_DIR = "Library/ScriptAssemblies";
        const string BUILDED_ASSEMBLIES_DIR = "Build/Build_Data/Managed";

        const string RUNTIME_PUBLISH_DIR = "Runtime/Plugins";
        const string EDITOR_PUBLISH_DIR = "Editor/Plugins";

        const string START_TO_DEST_PARSER = @"\s*>>>\s*";

        UPRTSetter setter;

        UnityAssetGUID[] publishPackageAssetGUIDs;
        UnityAssetGUID[] publishProjectAssetGUIDs;

        /// <summary>
        /// Unity Package Release Tool 생성자입니다.
        /// </summary>
        /// <param name="configPath">Helper Json 파일 디렉토리</param>
        /// <exception cref="ArgumentNullException">인스턴스 생성 예외처리</exception>
        /// <exception cref="FileNotFoundException">Config 파일 위치 예외처리</exception>
        public UPRTUnityHelper(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
                throw new ArgumentNullException("configPath", "UnityHelper를 생성에 실패했습니다.");

            if (!File.Exists(configPath))
                throw new FileNotFoundException($"UnityHelper ({configPath})의 파일을 찾을 수 없습니다.");

            string configJson = File.ReadAllText(configPath);

            this.setter = JsonConvert.DeserializeObject<UPRTSetter>(configJson);

            Debug.WriteLine("UPRTSetter 생성 성공");
        }

        /// <summary>
        /// 표준 방법 Unity Packaging
        /// </summary>
        /// <returns>실행 절차</returns>
        public IEnumerable<string> CommandStandard()
        {
            Log("START STANDARD PACKAGING COMMAND");

            yield return nameof(BuildUnitySource);
            if (BuildUnitySource() != SUCCESS_CODE)
                yield break;

            yield return nameof(GetSyncGUIDs);
            if (GetSyncGUIDs() != SUCCESS_CODE)
                yield break;

            yield return nameof(CopyPackageItems);
            if (CopyPackageItems() != SUCCESS_CODE)
                yield break;

            yield return nameof(GetSimulateSyncGUIDs);
            if (GetSimulateSyncGUIDs() != SUCCESS_CODE)
                yield break;

            yield return nameof(SyncGUIDs);
            if (SyncGUIDs() != SUCCESS_CODE)
                yield break;

            yield return nameof(CopySampleItems);
            if (CopySampleItems() != SUCCESS_CODE)
                yield break;

            Log("END STANDARD PACKAGING COMMAND");
        }

        /// <summary>
        /// Unity Package Project를 타겟하여, 내부 함수를 통해 관련 대상들을 DLL로 빌드합니다.
        /// </summary>
        /// <returns>종료 코드</returns>
        public int BuildUnitySource()
        {
            Log("Unity Project Source Build : Start");

            // Unity Editor를 실행시킬 프로세스 Setting
            Process process = new Process();

            process.StartInfo.FileName = this.setter.UnityPath;
            process.StartInfo.ArgumentList.Add("-quit");
            process.StartInfo.ArgumentList.Add("-batchmode");
            process.StartInfo.ArgumentList.Add("-logFile");
            process.StartInfo.ArgumentList.Add("-");
            process.StartInfo.ArgumentList.Add("-projectPath");
            process.StartInfo.ArgumentList.Add(this.setter.UnityProjectPath);
            process.StartInfo.ArgumentList.Add("-executeMethod");
            process.StartInfo.ArgumentList.Add(this.setter.BuildUnitySourceMethod);
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += Log_OutputReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            process.OutputDataReceived -= Log_OutputReceived;

            Log($"Unity Project Source Build : Finish ({GetResultMessage(process.ExitCode)})");

            return process.ExitCode;
        }

        /// <summary>
        /// Unity Project에서 Package 디렉토리로 아이템들을 복사합니다.
        /// </summary>
        /// <returns>종료 코드</returns>
        public int CopyPackageItems()
        {
            Log("Unity Project Source Copy : Start");

            foreach (var item in this.setter.PackageContent)
            {
                switch (item.ContentType)
                {
                    case Enums.ContentType.Runtime:
                        RuntimeCopier(item.Targets, item.CopyForce);
                        break;
                    case Enums.ContentType.Editor:
                        EditorCopier(item.Targets, item.CopyForce);
                        break;
                    case Enums.ContentType.TestProject:
                        TestProjectCopier(item.Targets);
                        break;
                    case Enums.ContentType.Package:
                        PackageCopier(item.Targets);
                        break;
                }
            }

            Log("Unity Project Source Copy : Finish");
            
            return SUCCESS_CODE;
        }

        /// <summary>
        /// Simulation Project에서 Package 디렉토리로 복사하는 함수입니다.
        /// </summary>
        /// <returns>종료 코드</returns>
        public int CopySampleItems()
        {
            Log("Unity Sample Source Copy : Start");

            foreach (var item in this.setter.PackageContent)
            {
                switch (item.ContentType)
                {
                    case Enums.ContentType.Sample:
                        SampleCopier(item.Targets, item.CopyForce);
                        break;
                }
            }

            Log("Unity Sample Source Copy : Finish");

            return SUCCESS_CODE;
        }

        /// <summary>
        /// Unity Package Project의 샘플들을 타겟하여 내부 함수를 통해 초기화하며, 그 결과를 패키지 결과 대상에 복사합니다.
        /// </summary>
        /// <returns>종료 코드</returns>
        public int BuildUnitySample()
        {
            Log("Unity Project Sample Build : Start");

            // Unity Editor를 실행시킬 프로세스 Setting
            Process process = new Process();

            process.StartInfo.FileName = this.setter.UnityPath;
            process.StartInfo.ArgumentList.Add("-quit");
            process.StartInfo.ArgumentList.Add("-batchmode");
            process.StartInfo.ArgumentList.Add("-logFile");
            process.StartInfo.ArgumentList.Add("-");
            process.StartInfo.ArgumentList.Add("-projectPath");
            process.StartInfo.ArgumentList.Add(this.setter.PublishTargetProjectPath);
            process.StartInfo.ArgumentList.Add("-executeMethod");
            // Run Unity Packaging Method -----------------------
            // process.StartInfo.ArgumentList.Add(excuteBuild);
            // --------------------------------------------------
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += Log_OutputReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            process.OutputDataReceived -= Log_OutputReceived;

            Log($"Unity Project Sample Build : Finish ({GetResultMessage(process.ExitCode)})");

            return process.ExitCode;
        }

        /// <summary>
        /// Unity Package Project 리소스를 타겟하여 내부 함수로 GUID를 얻어, UPRTSetter.Start에 지정된 Json 파일로 떨어뜨려 놓습니다.
        /// </summary>
        /// <returns>종료 코드</returns>
        public int GetSyncGUIDs()
        {
            Log("Unity Package Project Sync GUIDs : Start");

            // Unity Editor를 실행시킬 프로세스 Setting
            Process process = new Process();

            process.StartInfo.FileName = this.setter.UnityPath;
            process.StartInfo.ArgumentList.Add("-quit");
            process.StartInfo.ArgumentList.Add("-batchmode");
            process.StartInfo.ArgumentList.Add("-logFile");
            process.StartInfo.ArgumentList.Add("-");
            process.StartInfo.ArgumentList.Add("-projectPath");
            process.StartInfo.ArgumentList.Add(this.setter.UnityProjectPath);
            process.StartInfo.ArgumentList.Add("-executeMethod");
            process.StartInfo.ArgumentList.Add(this.setter.SyncGUIDsMethod);
            process.StartInfo.ArgumentList.Add(SYNC_GUID_TARGET_ARGS);
            process.StartInfo.ArgumentList.Add(Path.Combine(Directory.GetCurrentDirectory(), this.setter.GUIDSyncTargets.Start));
            process.StartInfo.ArgumentList.Add("-packageTitle");
            process.StartInfo.ArgumentList.Add(this.setter.PackageTitle);
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += Log_OutputReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            process.OutputDataReceived -= Log_OutputReceived;

            Log($"Unity Package Project Sync GUIDs : Finish ({GetResultMessage(process.ExitCode)})");

            return process.ExitCode;
        }

        /// <summary>
        /// Simulation Project 리소스를 타겟하여 내부 함수로 GUID를 얻어, UPRTSetter.Destination에 지정된 Json 파일로 떨어뜨려 놓습니다.
        /// </summary>
        /// <returns>종료 코드</returns>
        public int GetSimulateSyncGUIDs()
        {
            Log("Unity Package Test Project Sync GUIDs : Start");

            // Unity Editor를 실행시킬 프로세스 Setting
            Process process = new Process();

            process.StartInfo.FileName = this.setter.UnityPath;
            process.StartInfo.ArgumentList.Add("-quit");
            process.StartInfo.ArgumentList.Add("-batchmode");
            process.StartInfo.ArgumentList.Add("-logFile");
            process.StartInfo.ArgumentList.Add("-");
            process.StartInfo.ArgumentList.Add("-projectPath");
            process.StartInfo.ArgumentList.Add(this.setter.PublishTargetProjectPath);
            process.StartInfo.ArgumentList.Add("-executeMethod");
            process.StartInfo.ArgumentList.Add(this.setter.SyncGUIDsMethod);
            process.StartInfo.ArgumentList.Add(SYNC_GUID_TARGET_ARGS);
            process.StartInfo.ArgumentList.Add(Path.Combine(Directory.GetCurrentDirectory(), this.setter.GUIDSyncTargets.Destination));
            process.StartInfo.ArgumentList.Add("-packageTitle");
            process.StartInfo.ArgumentList.Add(this.setter.PackageTitle);
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += Log_OutputReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            process.OutputDataReceived -= Log_OutputReceived;

            Log($"Unity Package Test Project Sync GUIDs : Finish ({GetResultMessage(process.ExitCode)})");

            return process.ExitCode;
        }

        /// <summary>
        /// Package와 Test Project의 에셋 정보를 동일하게 설정합니다.
        /// </summary>
        /// <returns>종료 코드</returns>
        public int SyncGUIDs()
        {
            Log("Synchronize GUIDs : Start");
            ReplaceGUIDs();
            Log("Synchronize GUIDs : Finish");
            return SUCCESS_CODE;
        }

        void RuntimeCopier(string[] fileNames, bool force = true)
        {
            string projectPath = Path.Combine(this.setter.UnityProjectPath, BUILDED_ASSEMBLIES_DIR);
            string publishPath = Path.Combine(this.setter.PublishRootPath, RUNTIME_PUBLISH_DIR);

            foreach (var item in fileNames)
            {
                string target = Path.Combine(projectPath, item);
                string publish = Path.Combine(publishPath, item);

                CopyUtility.Copy(target, publish, force, Log);
            }
        }

        void EditorCopier(string[] fileNames, bool force = true)
        {
            string projectPath = Path.Combine(this.setter.UnityProjectPath, SCRIPT_ASSEMBLIES_DIR);
            string publishPath = Path.Combine(this.setter.PublishRootPath, EDITOR_PUBLISH_DIR);

            foreach (var item in fileNames)
            {
                string target = Path.Combine(projectPath, item);
                string publish = Path.Combine(publishPath, item);

                CopyUtility.Copy(target, publish, force, Log);
            }
        }

        void SampleCopier(string[] fileNames, bool force = true)
        {
            foreach (var item in fileNames)
            {
                string[] components = Regex.Split(item, START_TO_DEST_PARSER);

                if (components.Length != 2) throw new FormatException($"Sample File ({item})의 형식이 적절치 않습니다.");

                string target = Path.Combine(this.setter.PublishTargetProjectPath, components[0]);
                string publish = Path.Combine(this.setter.PublishRootPath, components[1]);

                CopyUtility.Copy(target, publish, force, Log);
            }
        }

        void TestProjectCopier(string[] fileNames, bool force = true)
        {
            foreach (var item in fileNames)
            {
                string[] components = Regex.Split(item, START_TO_DEST_PARSER);

                if (components.Length != 2) throw new FormatException($"Default File ({item})의 형식이 적절치 않습니다.");

                string target = Path.Combine(this.setter.UnityProjectPath, components[0]);
                string publish = Path.Combine(this.setter.PublishTargetProjectPath, components[1]);

                CopyUtility.Copy(target, publish, force, Log);
            }
        }

        void PackageCopier(string[] fileNames, bool force = true)
        {
            foreach (var item in fileNames)
            {
                string[] components = Regex.Split(item, START_TO_DEST_PARSER);

                if (components.Length != 2) throw new FormatException($"Package File ({item})의 형식이 적절치 않습니다.");

                string target = Path.Combine(this.setter.UnityProjectPath, PACKAGES_DIR, this.setter.PackageTitle, components[0]);
                string publish = Path.Combine(this.setter.PublishRootPath, components[1]);
                
                CopyUtility.Copy(target, publish, force, Log);
            }
        }

        void SetGUIDSyncTargets()
        {
            // Start sync
            using (StreamReader reader = new StreamReader(this.setter.GUIDSyncTargets.Start))
            {
                JArray jArray = JArray.Load(new JsonTextReader(reader));
                publishPackageAssetGUIDs = jArray.Children().
                    Select(token => token.ToObject<UnityAssetGUID>()).
                    Where(Guid => this.setter.GUIDSyncTargets.TargetNames.Contains(Guid.Name)).
                    ToArray();
            }

            // Publish sync
            using (StreamReader reader = new StreamReader(this.setter.GUIDSyncTargets.Destination))
            {
                JArray jArray = JArray.Load(new JsonTextReader(reader));
                publishProjectAssetGUIDs = jArray.Children().
                    Select(token => token.ToObject<UnityAssetGUID>()).
                    Where(Guid => this.setter.GUIDSyncTargets.TargetNames.Contains(Guid.Name)).
                    ToArray();
            }
        }

        // Error
        void ReplaceGUIDs()
        {
            SetGUIDSyncTargets();

            List<string> fileList = new List<string>();

            // Package GUID
            foreach (var extension in GuidSyncExtensions)
                fileList.AddRange(Directory.GetFiles(this.setter.PublishRootPath, extension, SearchOption.AllDirectories));

            // Publish Test Project GUID
            foreach (var extension in GuidSyncExtensions)
                fileList.AddRange(Directory.GetFiles(this.setter.PublishTargetProjectPath, extension, SearchOption.AllDirectories));

            var syncItem = from packageGUIDs in this.publishPackageAssetGUIDs
                           join testProjectGUIDs in this.publishProjectAssetGUIDs on packageGUIDs.Name equals testProjectGUIDs.Name
                           select new
                           {
                               Name = packageGUIDs.Name,
                               PackageGUID = packageGUIDs.GUID,
                               ProjectGUID = testProjectGUIDs.GUID,
                               PackageFileID = packageGUIDs.FileID,
                               ProjectFileID = testProjectGUIDs.FileID
                           };

            foreach (var fileItem in fileList)
            {
                bool overwrite = false;
                string[] fileLines = File.ReadAllLines(fileItem);

                for (int i = 0; i < fileLines.Length; i++)
                {
                    bool replaced = false;
                    string packageFileID = string.Empty;
                    string publishTestFileID = string.Empty;

                    // GUID Replace 작업
                    if (fileLines[i].Contains("guid: "))
                    {
                        int index = fileLines[i].IndexOf("guid: ") + 6;
                        string oldGUID = fileLines[i].Substring(index, 32);

                        var guidInformation = syncItem.FirstOrDefault(guid => guid.PackageGUID == oldGUID);

                        if (guidInformation != null)
                        {
                            fileLines[i] = fileLines[i].Replace(oldGUID, guidInformation.ProjectGUID);

                            packageFileID = guidInformation.PackageFileID.ToString();
                            publishTestFileID = guidInformation.ProjectFileID.ToString();

                            replaced = true;
                            overwrite = true;

                            Log($"Synchronize GUIDs : {fileItem}의 GUID를 갱신하였습니다 ({oldGUID} to {guidInformation.ProjectGUID}).");
                        }
                    }

                    // FileID Replace 작업
                    if (replaced && fileLines[i].Contains("fileID: "))
                    {
                        int index = fileLines[i].IndexOf("fileID: ") + 8;
                        int indexPivot = fileLines[i].IndexOf(",", index);
                        string oldFileID = fileLines[i].Substring(index, indexPivot - index);

                        if (packageFileID == oldFileID)
                        {
                            fileLines[i] = fileLines[i].Replace(oldFileID, publishTestFileID);
                            overwrite = true;

                            Log($"Synchronize GUIDs : {fileItem}의 GUID를 갱신하였습니다 ({oldFileID} to {publishTestFileID}).");
                        }
                    }
                }

                if (overwrite)
                {
                    File.WriteAllLines(fileItem, fileLines);
                    Log($"Synchronize GUIDs : {fileItem}의 파일이 최종 갱신되었습니다.");
                }
            }
        }

        string BuildMessage(string message, string prefix = "")
        {
            if (string.IsNullOrEmpty(prefix))
                return $"[UPRTUnityHelper] {message}";
            else
                return $"[UPRTUnityHelper - {prefix}] {message}";
        }

        string GetResultMessage(int resultCode, string success = "", string fail = "")
        {
            string successMsg = "Success";
            string failMsg = "Fail";
            if (!string.IsNullOrEmpty(success)) successMsg = success;
            if (!string.IsNullOrEmpty(fail)) failMsg = fail;
            return (resultCode == SUCCESS_CODE) ? successMsg : failMsg;
        }

        void Log(string message)
        {
            ConsoleLog.Log(BuildMessage(message));
        }

        void Log(string message, string prefix)
        {
            ConsoleLog.Log(BuildMessage(message, prefix));
        }

        void Log_OutputReceived(object sender, DataReceivedEventArgs e)
        {
            Log(e.Data, "Process Output");
        }

        void LogWarning(string message)
        {

        }

        
    }
}
