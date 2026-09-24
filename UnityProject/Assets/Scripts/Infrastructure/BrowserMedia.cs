using System.IO;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Infrastructure
{
    public static class BrowserMedia
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void PickBrowserMedia(string receiver, string directory, string kind);
        [DllImport("__Internal")] private static extern void PlayBrowserAudio(string path);
        [DllImport("__Internal")] private static extern void StopBrowserAudio();
        [DllImport("__Internal")] private static extern void SyncBrowserFiles();

        public static void Pick(GameObject receiver, string kind) =>
            PickBrowserMedia(receiver.name, MediaDirectory, kind);

        public static void PlayAudio(string path) => PlayBrowserAudio(path);
        public static void StopAudio() => StopBrowserAudio();
        public static void Sync() => SyncBrowserFiles();

        public static void DeleteImported(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith(MediaDirectory + "/")) return;
            if (!File.Exists(path)) return;
            File.Delete(path);
            Sync();
        }

        private static string MediaDirectory => Path.Combine(UnityEngine.Application.persistentDataPath, "media").Replace('\\', '/');
#else
        public static void Pick(GameObject receiver, string kind) { }
        public static void PlayAudio(string path) { }
        public static void StopAudio() { }
        public static void Sync() { }
        public static void DeleteImported(string path) { }
#endif
    }
}
