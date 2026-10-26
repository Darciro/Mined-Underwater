using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Runs once per Editor session and flags repository problems that cause mass compile/GUID errors:
/// Git LFS pointer files that were never downloaded, and third-party packages duplicated outside Assets/_Imports.
/// </summary>
[InitializeOnLoad]
public static class ProjectHealthCheck
{
    private const string SessionKey = "ProjectHealthCheck.Ran";
    private const string LfsPointerHeader = "version https://git-lfs";
    private const int MaxPointerSize = 200;

    private static readonly string[] LfsExtensions =
    {
        ".png", ".jpg", ".jpeg", ".psd", ".tga", ".tif", ".tiff", ".gif", ".bmp", ".exr", ".hdr",
        ".wav", ".mp3", ".ogg", ".aif", ".aiff", ".fbx", ".obj", ".blend", ".ttf", ".otf",
        ".dylib", ".so", ".a", ".aar", ".mp4", ".mov"
    };

    static ProjectHealthCheck()
    {
        if (SessionState.GetBool(SessionKey, false)) return;
        SessionState.SetBool(SessionKey, true);
        EditorApplication.delayCall += () => Run(false);
    }

    [MenuItem("Tools/Project Health Check")]
    private static void RunFromMenu() => Run(true);

    private static void Run(bool reportSuccess)
    {
        List<string> lfsPointers = FindLfsPointers();
        List<string> duplicates = FindDuplicatedImports();

        if (lfsPointers.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[ProjectHealthCheck] {lfsPointers.Count} Git LFS files were not downloaded (pointer files only).");
            sb.AppendLine("Close Unity and run: brew install git-lfs && git lfs install && git lfs pull");
            foreach (string path in lfsPointers.Take(10)) sb.AppendLine("  " + path);
            Debug.LogError(sb.ToString());
        }

        if (duplicates.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[ProjectHealthCheck] Third-party packages exist both in Assets/ and Assets/_Imports/ (duplicate GUIDs).");
            sb.AppendLine("Close Unity, delete the copies outside _Imports, run 'git checkout -- Assets/_Imports' and delete Library/.");
            foreach (string path in duplicates) sb.AppendLine("  " + path);
            Debug.LogError(sb.ToString());
        }

        if (reportSuccess && lfsPointers.Count == 0 && duplicates.Count == 0)
        {
            Debug.Log("[ProjectHealthCheck] No problems found.");
        }
    }

    private static List<string> FindLfsPointers()
    {
        var result = new List<string>();
        var header = new byte[LfsPointerHeader.Length];

        foreach (string path in Directory.EnumerateFiles("Assets", "*", SearchOption.AllDirectories))
        {
            if (!LfsExtensions.Contains(Path.GetExtension(path).ToLowerInvariant())) continue;
            if (new FileInfo(path).Length > MaxPointerSize) continue;

            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Read(header, 0, header.Length) == header.Length &&
                    Encoding.ASCII.GetString(header) == LfsPointerHeader)
                {
                    result.Add(path);
                }
            }
        }

        return result;
    }

    private static List<string> FindDuplicatedImports()
    {
        const string importsRoot = "Assets/_Imports";
        if (!Directory.Exists(importsRoot)) return new List<string>();

        return Directory.GetDirectories(importsRoot)
            .Select(Path.GetFileName)
            .Where(name => Directory.Exists(Path.Combine("Assets", name)))
            .Select(name => "Assets/" + name)
            .ToList();
    }
}
