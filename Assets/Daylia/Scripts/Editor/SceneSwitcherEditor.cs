using UnityEngine;
using UnityEditor; // C'est la directive manquante!

// Indique à Unity que c'est un éditeur personnalisé pour le script SceneSwitcher
[CustomEditor(typeof(SceneSwitcher))]
public class SceneSwitcherEditor : Editor
{
    // Référence au SceneAsset pour le glisser-déposer dans l'éditeur
    private SceneAsset sceneAsset;
    
    // Référence au string qui stockera le nom de la scène dans le script de jeu
    private SerializedProperty sceneNameProperty;

    private void OnEnable()
    {
        // Initialise les références aux propriétés du script SceneSwitcher 
        sceneNameProperty = serializedObject.FindProperty("sceneName"); 

        // On vérifie si le nom de scène est déjà défini.
        SceneSwitcher t = (SceneSwitcher)target; // On récupère une référence au script de jeu
        
        if (!string.IsNullOrEmpty(t.sceneName))
        {
             // Si le nom existe, on cherche l'asset correspondant dans le projet
             string[] guids = AssetDatabase.FindAssets("t:SceneAsset " + t.sceneName);
             if (guids.Length > 0)
             {
                 string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                 sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
             }
        }
    }

    public override void OnInspectorGUI()
    {
        // Dessine toutes les autres propriétés du script SceneSwitcher
        DrawDefaultInspector(); 
        
        // Assure que les dernières valeurs sont chargées
        serializedObject.Update(); 

        // Dessine le champ SceneAsset pour le glisser-déposer
        EditorGUI.BeginChangeCheck();
        SceneAsset newSceneAsset = EditorGUILayout.ObjectField("Scene To Load (Drag & Drop)", sceneAsset, typeof(SceneAsset), false) as SceneAsset;
        if (EditorGUI.EndChangeCheck())
        {
            // Met à jour notre référence locale
            sceneAsset = newSceneAsset;

            // Si l'utilisateur a déposé une nouvelle scène, met à jour les deux propriétés
            if (newSceneAsset != null)
            {
                // Mettre à jour la chaîne de caractères du script de jeu
                sceneNameProperty.stringValue = newSceneAsset.name; 
            }
            else
            {
                // Si l'Asset est retiré, le string redevient vide
                sceneNameProperty.stringValue = "";
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}