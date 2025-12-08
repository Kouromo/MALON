using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    // Le Singleton
    public static DataManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Garde le DataManager entre les scènes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Chemin de sauvegarde (Application.persistentDataPath est l'emplacement recommandé pour les sauvegardes)
    private string GetPathForUser(string email)
    {
        // On utilise l'email (ou un ID unique) comme nom de fichier
        // On remplace les caractères invalides pour le nom de fichier
        string safeEmail = email.Replace("@", "_at_").Replace(".", "_dot_");
        return Path.Combine(Application.persistentDataPath, safeEmail + "_profile.json");
    }

    public void SetSessionData(UserProfile profile)
    {
        // 1. Stocker les données dans PlayerPrefs pour l'accès facile pendant la session
        PlayerPrefs.SetString("User_FirstName", profile.firstName);
        PlayerPrefs.SetString("User_LastName", profile.lastName);
        PlayerPrefs.SetString("User_Email", profile.email);
        PlayerPrefs.SetString("User_Position", profile.position);
        
        // On pourrait aussi sauvegarder le hash pour ne pas le recharger du JSON
        PlayerPrefs.SetString("User_PasswordHash", profile.passwordHash); 

        PlayerPrefs.Save(); // Assurez-vous d'appeler Save()
        Debug.Log("PlayerPrefs de session mis à jour.");
    }

    public void SaveProfile(UserProfile profile)
    {
        string path = GetPathForUser(profile.email);

        // 1. Convertir l'objet C# en chaîne JSON
        string json = JsonUtility.ToJson(profile);

        // 2. Écrire la chaîne JSON dans le fichier
        File.WriteAllText(path, json);

        Debug.Log("Profil sauvegardé à : " + path);
    }

    public UserProfile LoadProfile(string email)
    {
        string path = GetPathForUser(email);

        // 1. Vérifier si le fichier existe
        if (File.Exists(path))
        {
            // 2. Lire tout le contenu du fichier (la chaîne JSON)
            string json = File.ReadAllText(path);

            // 3. Convertir la chaîne JSON en objet UserProfile C#
            UserProfile loadedProfile = JsonUtility.FromJson<UserProfile>(json);

            return loadedProfile;
        }
        else
        {
            Debug.LogWarning("Aucun profil trouvé pour l'email : " + email);
            return null; // Retourne null si le profil n'existe pas
        }
    }

    public bool VerifyLogin(string email, string enteredPassword)
    {
        UserProfile profile = LoadProfile(email);

        if (profile != null)
        {
            // *4. Comparer le HASH du mot de passe entré avec le HASH sauvegardé.
            // Vous auriez besoin d'une fonction de hachage ici.
            // Par simplicité, je simule une vérification :
            string enteredPasswordHash = HashFunction(enteredPassword); 

            if (enteredPasswordHash == profile.passwordHash)
            {
                // Connexion réussie : charger les données dans PlayerPrefs pour la session
                SetSessionData(profile);
                return true;
            }
        }
        
        return false;
    }

    private string HashFunction(string password)
    {
        // *Implémenter une fonction de hachage (hors proto)*
        return password; 
    }
}