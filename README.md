# Unity Package Release Tool

Unity Package 배포를 관리할 수 있도록 구현된 툴입니다. C# 코드를 DLL빌드하며, 이 결과를 패키지 배포하는 프로젝트에 대해 타겟되어 사용할 수 있게 구성되었습니다.

## 디렉토리 구성

```bash
├── PackageTool : 배포용 툴
├── UPRT : 패키지 대상 프로젝트
└── UPRT Publish : 최종 패키지 그리고 산출된 패키지 테스트 프로젝트의 디렉토리
    ├── UPRT : 배포될 최종 패키지 디렉토리
    └── UPRT Publish Test Project : 실제 패키지가 적용된 상황을 시뮬레이트한 프로젝트
```

## 사용 가이드

1. 3개의 디렉토리 (패키지 작업이 진행된 프로젝트, 해당 패키지를 테스트할 수 있는 테스트 프로젝트, 그리고 패키지 최종 디렉토리를 구성합니다.

2. 두 Unity Project (패키지 작업이 진행된 프로젝트, 그리고 테스트할 수 있는 테스트 프로젝트)에 동봉된 ``UPRT Unity Package.unitypackage``를 Import 합니다.

3. 패키지 최종 디렉토리에 ``package.json`` 파일 구성하여, 테스트 프로젝트에서 upm으로 해당 패키지를 추가.

4. ``PackageTool``의 프로젝트를 열어, 대상 프로젝트에 맞게 ``UPRTUnityHelper.json``을 수정합니다.

5. ``PackageTool`` 프로젝트를 빌드한 후, 실행합니다 (표준 방법 : *standard* 명령어).

## npm 사용

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