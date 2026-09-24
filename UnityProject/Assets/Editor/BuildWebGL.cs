using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildWebGL
{
    public static void Run()
    {
        var args = Environment.GetCommandLineArgs();
        var outputOption = Array.IndexOf(args, "-webglOutput");
        if (outputOption < 0 || outputOption + 1 >= args.Length)
            throw new ArgumentException("Pass -webglOutput with an output directory.");

        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length == 0) throw new InvalidOperationException("No enabled scenes in build settings.");

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        var output = Path.GetFullPath(args[outputOption + 1]);
        Directory.CreateDirectory(output);
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.WebGL, BuildOptions.None);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception($"WebGL build failed: {report.summary.result}");

        Debug.Log($"WebGL build complete: {output}");
    }
}
