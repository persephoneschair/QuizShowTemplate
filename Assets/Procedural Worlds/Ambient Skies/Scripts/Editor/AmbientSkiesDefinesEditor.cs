using UnityEditor;

namespace AmbientSkies
{
    [InitializeOnLoad]
    public class AmbientSkiesDefinesEditor :Editor
    {
        static AmbientSkiesDefinesEditor()
        {
            bool isChanged = false;

            //Gets the scripting defines
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!currBuildSettings.Contains("AMBIENT_SKIES"))
            {
                if (string.IsNullOrEmpty(currBuildSettings))
                {
                    currBuildSettings += "AMBIENT_SKIES";
                    isChanged = true;
                }
                else
                {
                    currBuildSettings += ";AMBIENT_SKIES";
                    isChanged = true;
                }
            }

            if (isChanged)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
            }
        }
    }
}