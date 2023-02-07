using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPRT.Standard.Utility
{
    internal class CopyUtility
    {
        public static void Copy(string start, string dest, bool force, Action<string> action = null)
        {
            if (File.GetAttributes(start).HasFlag(FileAttributes.Directory))
            {
                CopyDirectory(start, dest, force, action);
            }
            else
            {
                CopyFile(start, dest, force, action);
            }
        }

        public static void CopyDirectory(string start, string dest, bool force, Action<string> action)
        {
            DirectoryInfo startDirInfo = new DirectoryInfo(start);

            if (!startDirInfo.Exists)
                throw new DirectoryNotFoundException($"Directory를 찾을 수 없습니다 : {startDirInfo.FullName}");

            if (Directory.Exists(dest))
            {
                if (!force)
                {
                    action?.Invoke($"존재하는 Directory이므로, 설정 (force : {force})에 따라서 스킵합니다 : {dest}");
                    return;
                }
                else
                {
                    action?.Invoke($"존재하는 Directory이므로, 설정 (force : {force}에 따라서 삭제합니다 : {dest}");
                    Directory.Delete(dest, true);
                }
            }

            action?.Invoke($"Directory를 생성합니다 : {dest}");
            Directory.CreateDirectory(dest);

            // 파일 복사
            foreach (FileInfo file in startDirInfo.GetFiles())
            {
                CopyFile(file.FullName, Path.Combine(dest, file.Name), force, action);
            }

            // 디렉토리 복사
            DirectoryInfo[] startDirInfos = startDirInfo.GetDirectories();
            foreach (DirectoryInfo subDir in startDirInfos)
            {
                CopyDirectory(subDir.FullName, Path.Combine(dest, subDir.Name), force, action);
            }
        }

        public static void CopyFile(string start, string dest, bool force, Action<string> action)
        {
            FileInfo startFileInfo = new FileInfo(start);

            if (!startFileInfo.Exists)
                throw new FileNotFoundException($"File을 찾을 수 없습니다 : {startFileInfo.FullName}");

            if (File.Exists(dest))
            {
                if (!force)
                {
                    action?.Invoke($"존재하는 File이므로, 설정 (force : {force})에 따라서 스킵합니다 : {dest}");
                    return;
                }
                else
                {
                    action?.Invoke($"존재하는 File이므로, 설정 (force : {force}에 따라서 삭제합니다 : {dest}");
                    File.Delete(dest);
                }
            }

            action.Invoke($"File을 복사합니다 : {startFileInfo.FullName} => {dest}");

            FileInfo destFileInfo = new FileInfo(dest);

            if (!Directory.Exists(destFileInfo.Directory.FullName))
                Directory.CreateDirectory(destFileInfo.Directory.FullName);
            startFileInfo.CopyTo(dest);
        }
    }
}
