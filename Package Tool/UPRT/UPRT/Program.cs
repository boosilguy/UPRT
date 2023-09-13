using System;
using UPRT.Standard;

namespace UPRT
{
    class Program
    {
        public const string Command_Standard = "standard";
        public const string Command_Build = "build";
        public const string Command_GetGUID = "getguid";
        public const string Command_Copy = "copy";
        public const string Command_GetSimulateGUID = "getsimguid";
        public const string Command_SyncGUID = "sync";
        public const string Command_Sample = "sample";
        public const string Command_Help = "help";

        static void Main(string[] args)
        {
            try
            {
                ConsoleLog.Create(Console.Out);
                var uprtHelper = new UPRTUnityHelper("UPRTUnityHelper.json");

                if (args.Length == 1)
                {
                    string inputCmd = args.First<string>().ToLower();

                    switch(inputCmd)
                    {
                        case Command_Standard:
                            foreach (var func in uprtHelper.CommandStandard())
                            {
                                string executeFuncName = GetExecuteFuncName(func);
                                Console.WriteLine($"테이블에 올라간 작업 내용 ({executeFuncName})");
                            }
                            break;
                        case Command_Build:
                            uprtHelper.BuildUnitySource();
                            break;
                        case Command_GetGUID:
                            uprtHelper.GetSyncGUIDs();
                            break;
                        case Command_Copy:
                            uprtHelper.CopyPackageItems();
                            break;
                        case Command_GetSimulateGUID:
                            uprtHelper.GetSimulateSyncGUIDs();
                            break;
                        case Command_SyncGUID:
                            uprtHelper.SyncGUIDs();
                            break;
                        case Command_Sample:
                            uprtHelper.CopySampleItems();
                            break;
                        case Command_Help:
                            PrintGuide();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        static string GetExecuteFuncName(string funcName)
        {
            switch (funcName)
            {
                case nameof(UPRTUnityHelper.BuildUnitySource):
                    return "Build Unity Source";
                case nameof(UPRTUnityHelper.GetSyncGUIDs):
                    return "Get Package Sync GUIDs";
                case nameof(UPRTUnityHelper.CopyPackageItems):
                    return "Copy Package Items";
                case nameof(UPRTUnityHelper.GetSimulateSyncGUIDs):
                    return "Get Package Test Project Sync GUIDs";
                case nameof(UPRTUnityHelper.SyncGUIDs):
                    return "Synchronize GUIDs";
                case nameof(UPRTUnityHelper.CopySampleItems):
                    return "Copy Sample Items";
            }

            return string.Empty;
        }

        static void PrintGuide()
        {
            Console.WriteLine("-- 실행 명령어");
            Console.WriteLine("-- ");
            Console.WriteLine("-- standard : 표준 방법으로 패키징 작업을 시작합니다.");
            Console.WriteLine("-- ");
            Console.WriteLine("-- build : Package가 포함된 Unity Project의 Build 명령을 실행하여, DLL 파일을 구성합니다.");
            Console.WriteLine("-- ");
            Console.WriteLine("-- getguid : Unity Project 내 오브젝트의 GUID를 추출합니다.");
            Console.WriteLine("-- ");
            Console.WriteLine("-- copy : 빌드된 Unity Project의 아이템을 Simulation Project에 복사해 놓습니다.");
            Console.WriteLine("-- ");
            Console.WriteLine("-- getsimguid : Simulation Project 내 오브젝트의 GUID를 추출합니다.");
            Console.WriteLine("-- ");
            Console.WriteLine("-- sync : Unity, 그리고 Simulation Project 내 오브젝트의 GUID를 일치화 시킵니다.");
            Console.WriteLine("-- ");
            Console.WriteLine("-- sample : Simulation Project의 Sample을 Package 디렉토리로 복사합니다.");
        }
    }
}