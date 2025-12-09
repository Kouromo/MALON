using System;
using UnityEngine;

[Serializable]
public class UserProfile
{
    // Tous les champs que vous souhaitez sauvegarder
    public string firstName;
    public string lastName;
    public string email;
    public string passwordHash; // *Astuce : Ne jamais stocker de mots de passe en clair !
    public string position; // Poste

    // Constructeur pour l'initialisation lors de l'inscription
    public UserProfile(string first, string last, string mail, string pHash, string pos)
    {
        firstName = first;
        lastName = last;
        email = mail;
        passwordHash = pHash;
        position = pos;
    }

    // Constructeur par défaut nécessaire pour la désérialisation
    public UserProfile() { }

}