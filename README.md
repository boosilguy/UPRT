# Unity Package Release Tool

Unity Package 배포를 관리할 수 있도록 구현된 툴입니다. C# 코드를 DLL빌드하며, 이 결과를 패키지 배포하는 프로젝트에 대해 타겟되어 사용할 수 있게 구성되었습니다.

## About

```bash
├── PackageTool : 배포용 툴
├── UPRT : 패키지 대상 프로젝트
└── UPRT Publish : 최종 패키지 그리고 산출된 패키지 테스트 프로젝트의 디렉토리
    ├── UPRT : 배포될 최종 패키지 디렉토리
    └── UPRT Publish Test Project : 실제 패키지가 적용된 상황을 시뮬레이트한 프로젝트
```

## Guide

1. 3개의 디렉토리 (패키지 작업이 진행된 프로젝트, 해당 패키지를 테스트할 수 있는 테스트 프로젝트, 그리고 패키지 최종 디렉토리를 구성합니다.

2. 두 Unity Project (패키지 작업이 진행된 프로젝트, 그리고 테스트할 수 있는 테스트 프로젝트)에 동봉된 ``UPRT Unity Package.unitypackage``를 Import 합니다.

3. 패키지 최종 디렉토리에 ``package.json`` 파일 구성하여, 테스트 프로젝트에서 upm으로 해당 패키지를 추가.

4. ``PackageTool``의 프로젝트를 열어, 대상 프로젝트에 맞게 ``UPRTUnityHelper.json``을 수정합니다.

5. ``PackageTool`` 프로젝트를 빌드한 후, 실행합니다 (표준 방법 : *standard* 명령어).

## Self-feedback

사내에서 제작한 Unity SDK를 UPM로 배포하기 위해 고안한 툴. Unity는 UUID 때문에 기존 디렉토리에서 작업물에 업데이트된 SDK를 엎어치면, Resource간의 Missing Link가 발생하는 오래된 이슈를 갖고 있었다. 이 점을 해결하면서 어떻게 DLL로 말아 배포할 수 있을까를 고민하였다.

우리는 SDK가 작업된 결과물을 마치 패키지가 배포되었을 때, UPM이나 Tar로 Import한 결과를 모의한 Test Project를 구성하였다. 여기서 두 프로젝트간 UUID를 Sync하여 (Test Project를 작업된 결과물의 UUID로 Sync), Test Project가 타겟하고 있는 SDK 디렉토리를 배포하게끔 구성하였다.

사실, Unity 2020.3 버전을 타겟으로 구성한 프로젝트지만, 21.3 버전에서는 ScriptAssemblies 디렉토리의 DLL을 타겟해야함을 뒤늦게 파악하였다. 20.3 버전을 바탕으로 작업하였을 땐, PlayerDataCache의 DLL을 사용했지만 (Editor 전처리기 코드 때문), 21.3 버전에서는 PlayerDataCache에 DLL이 떨어지지 않아, ScriptAssemblies내 DLL을 사용했다. 이 경우에는 Editor code도 동봉되어, SDK를 Import하였을 때 Editor에서 예상치 못한 기능이나 로직이 돌아갈 수 있다. 이 점을 조금 더 연구해 볼 필요가 있겠다. 

## NPM

- tarball
  > npm pack
- Login
  > npm login --registry http://sample.co.kr
- Publish
  > npm publish --registry http://sample.co.kr
- UnPublish
  > npm unpublish <패키지명>@<버전>
- 기본 registry 지정
  > npm config set registry http://sample.co.kr
  > npm publish
- 기본 registry 확인
  > npm config get registry
