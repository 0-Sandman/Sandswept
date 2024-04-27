using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace RoR2
{
    [CustomPropertyDrawer(typeof(PrefabReferenceAttribute))]
    public class PrefabReferencePropertyDrawer : PropertyDrawer
    {
        private static GameObject ConvertToPrefab(GameObject sceneObject)
        {
            PrefabStage prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(sceneObject);
            if (prefabStage == null)
            {
                return null;
            }

            string relativePath = Util.BuildPrefabTransformPath(sceneObject.transform.root, sceneObject.transform, false);
            string assetPath = prefabStage.prefabAssetPath;
            if (assetPath != null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                GameObject prefabChild = prefab?.transform.Find(relativePath)?.gameObject;
                return prefabChild;
            }
            return null;
        }

        private GameObject memoizedSceneObject;
        private GameObject memoizedPrefabObject;

        private GameObject GetDraggedPrefabObject()
        {
            UnityEngine.Object[] objectReferences = DragAndDrop.objectReferences;
            if (objectReferences.Length == 1)
            {
                if (objectReferences[0] is GameObject gameObject)
                {
                    if (!ReferenceEquals(memoizedSceneObject, gameObject))
                    {
                        memoizedSceneObject = gameObject;
                        memoizedPrefabObject = ConvertToPrefab(memoizedSceneObject);
                    }
                    return memoizedPrefabObject;
                }
            }
            return null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // base.OnGUI(position, property, label);
            EditorGUI.PropertyField(position, property, label, true);
            if (position.Contains(Event.current.mousePosition))
            {
                switch (Event.current.type)
                {
                    case EventType.DragUpdated:
                    {
                        GameObject draggedObject = GetDraggedPrefabObject();
                        if (draggedObject)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        }
                    } break;
                    case EventType.DragPerform:
                    {
                        GameObject draggedObject = GetDraggedPrefabObject();
                        if (draggedObject)
                        {
                            DragAndDrop.AcceptDrag();
                            property.objectReferenceValue = draggedObject;
                        }
                    } break;
                }
            }
        }
    }
}
