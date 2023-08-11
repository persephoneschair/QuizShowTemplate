using UnityEditor;
using PWCommon3;
using AmbientSkies.Internal;

namespace AmbientSkies
{
    [CustomEditor(typeof(AmbientSkiesSceneSavedSettings))]
    public class AmbientSkiesSceneSavedSettingsEditor : PWEditor
    {
        private EditorUtils m_editorUtils;

        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }
        }

        private void OnEnable()
        {
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
        }

        public override void OnInspectorGUI()
        {
            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!

            //Get Time Of Day object
            AmbientSkiesSceneSavedSettings profile = (AmbientSkiesSceneSavedSettings)target;

            //Monitor for changes
            EditorGUI.BeginChangeCheck();

            m_editorUtils.Panel("GlobalSettings", GlobalPanelSettings, true);

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(profile, "Made Changes");
                EditorUtility.SetDirty(profile);
            }
        }

        private void GlobalPanelSettings(bool helpEnabled)
        {
            m_editorUtils.Text("SavedSettingsInfo");
        }
    }
}