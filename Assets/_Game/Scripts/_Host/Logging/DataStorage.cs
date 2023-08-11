using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Linq;

public static class DataStorage
{
    private static string dirPath = "";
    private static int iteration = 1;

    public static void CreateDataPath()
    {
        dirPath = @$"{Application.persistentDataPath}/{DateTime.Now.ToShortDateString().Replace('/', '-')}";
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        else
        {
            string oldDirPath = dirPath;
            dirPath = $"{oldDirPath} [{iteration}]";
            
            while (Directory.Exists(dirPath))
            {
                iteration++;
                dirPath = $"{oldDirPath} [{iteration}]";
            }
            Directory.CreateDirectory(dirPath);
        }            
    }

    public static void SaveFile(string filename, string file)
    {
        if (dirPath == "")
            return;

        File.WriteAllText($"{dirPath}/{filename}.txt", file);
    }
}
