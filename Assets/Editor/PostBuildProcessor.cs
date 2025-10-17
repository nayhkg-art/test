using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class PostBuildProcessor
{
    // ビルドが完了した後に、このメソッドが自動的に呼び出されます
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        // ビルドターゲットがiOS以外の場合は何もしない
        if (target != BuildTarget.iOS)
        {
            return;
        }

        Debug.Log("iOSビルド完了。StoreKitファイルのコピーを開始します...");

        // コピー元のファイルパス (Unityプロジェクト内のパス)
        string sourcePath = "Assets/Editor/StoreKit/Products.storekit";

        // コピー先のファイルパス (ビルドされたXcodeプロジェクト内のパス)
        // プロジェクト名と同じ階層に配置されるように設定
        string destinationPath = Path.Combine(pathToBuiltProject, "Products.storekit");

        // ファイルが存在するか確認
        if (File.Exists(sourcePath))
        {
            // ファイルをコピーする (既に存在する場合は上書き)
            File.Copy(sourcePath, destinationPath, true);
            Debug.Log($"StoreKitファイルをコピーしました: {destinationPath}");
        }
        else
        {
            Debug.LogError($"StoreKitファイルが見つかりません: {sourcePath}");
        }
    }
}