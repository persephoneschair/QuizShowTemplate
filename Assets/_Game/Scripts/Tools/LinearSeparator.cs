using UnityEditor;
using UnityEngine;

namespace ATB
{
    [ExecuteInEditMode]
    public class LinearSeparator : MonoBehaviour
    {
        public Vector3 Separation = Vector3.zero;
        public bool Centered = false;

#if UNITY_EDITOR
        private void Awake()
        {
            if (!Application.isPlaying)
            {
                EditorApplication.update += Update;
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }

            int childCount = transform.childCount;
            Vector3 localRoot = Vector3.zero;
            if (Centered)
            {
                localRoot = Separation * (childCount - 1) * -0.5f;
            }

            for (int childIndex = 0; childIndex < childCount; ++childIndex)
            {
                transform.GetChild(childIndex).localPosition = localRoot;
                localRoot += Separation;
            }
        }
#endif
    }
}
