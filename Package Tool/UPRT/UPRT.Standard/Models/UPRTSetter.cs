using System;
using UPRT.Standard.Enums;

namespace UPRT.Standard.Models
{
    class UPRTSetter
    {
        /// <summary>
        /// 배포할 패키지 이름 (Package 파일의 루트가 될 폴더명)
        /// </summary>
        public string PackageTitle { get; set; }
        /// <summary>
        /// BuildUnitySource 명령을 실행시킬 executeMethod 메소드
        /// </summary>
        public string BuildUnitySourceMethod { get; set; }
        /// <summary>
        /// BuildUnitySource 명령을 실행시킬 executeMethod 메소드
        /// </summary>
        public string SyncGUIDsMethod { get; set; }
        /// <summary>
        /// Unity 설치 경로
        /// </summary>
        public string UnityPath { get; set; }
        /// <summary>
        /// Unity 프로젝트 경로
        /// </summary>
        public string UnityProjectPath { get; set; }
        /// <summary>
        /// 배포 프로젝트 루트 경로
        /// </summary>
        public string PublishRootPath { get; set; }
        /// <summary>
        /// 도착할 패키지가 포함된 Unity 프로젝트 경로 (테스트 프로젝트)
        /// </summary>
        public string PublishTargetProjectPath { get; set; }
        /// <summary>
        /// 패키지 구성 파일 콘텐츠
        /// </summary>
        public Content[] PackageContent { get; set; }
        /// <summary>
        /// Unity GUID Synchronize
        /// </summary>
        public GUIDSyncTargets GUIDSyncTargets { get; set; }
    }

    class Content
    {
        /// <summary>
        /// Content 이름
        /// </summary>
        public string Name { get; set; } = "NoTitle";
        /// <summary>
        /// Content Type
        /// </summary>
        public ContentType ContentType { get; set; }
        /// <summary>
        /// 복사 강제
        /// </summary>
        public bool CopyForce { get; set; } = true;
        /// <summary>
        /// 대상 파일 디렉토리
        /// </summary>
        public string[] Targets { get; set; }
    }

    class GUIDSyncTargets
    {
        public string Start { get; set; }
        public string Destination { get; set; }
        public List<string> TargetNames { get; set; }
    }

    class UnityAssetGUID
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
}
