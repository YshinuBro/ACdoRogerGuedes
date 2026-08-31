using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Gera o APK pela linha de comando, sem precisar clicar no Build Profiles.
//
//   Unity.exe -batchmode -quit -projectPath <projeto> \
//             -executeMethod BuildAndroid.Gerar -logFile <log>
//
// O caminho de saida pode vir em -customBuildPath; senao cai no padrao.
public static class BuildAndroid
{
    private const string SAIDA_PADRAO = "C:/Users/25011938/Downloads/OVigia-VR.apk";

    [MenuItem("O Vigia/Gerar APK", false, 60)]
    public static void Gerar()
    {
        string saida = CaminhoPedido();

        var cenas = new string[]
        {
            "Assets/Scenes/00_Menu.unity",
            "Assets/Scenes/01_Fase.unity"
        };

        var opcoes = new BuildPlayerOptions
        {
            scenes = cenas,
            locationPathName = saida,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };

        Debug.Log("[BuildAndroid] gerando em " + saida);
        BuildReport relatorio = BuildPipeline.BuildPlayer(opcoes);

        var resumo = relatorio.summary;
        Debug.Log("[BuildAndroid] resultado=" + resumo.result
                + " tamanho=" + (resumo.totalSize / (1024 * 1024)) + " MB"
                + " erros=" + resumo.totalErrors
                + " duracao=" + resumo.totalTime);

        if (resumo.result != BuildResult.Succeeded)
        {
            Debug.LogError("[BuildAndroid] FALHOU");
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }

    private static string CaminhoPedido()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-customBuildPath") return args[i + 1];
        }
        return SAIDA_PADRAO;
    }
}
