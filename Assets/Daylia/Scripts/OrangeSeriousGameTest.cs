using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OrangeSeriousGames : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public TMP_Dropdown jobDropdown;     // Vendeur / Technicien / Manager
    public Button vraiButton;
    public Button fauxButton;

    [Header("Config")]
    public int questionsParJour = 4;

    [System.Serializable]
    public class QuestionVF
    {
        public string texte;
        public bool reponse;  // true = VRAI, false = FAUX
    }

    private List<QuestionVF> questionsVendeur = new();
    private List<QuestionVF> questionsTechnicien = new();
    private List<QuestionVF> questionsManager = new();
    private List<QuestionVF> questionsCourantes = new();

    private int indexQuestion = 0;
    private int score = 0;

    void Start()
    {
        // 1. Initialiser les questions en dur pour l’instant
        InitQuestions();

        // 2. Fixer le dropdown (labels)
        jobDropdown.ClearOptions();
        jobDropdown.AddOptions(new List<string> { "Vendeur", "Technicien", "Manager" });
        jobDropdown.value = 0;
        jobDropdown.RefreshShownValue();

        // 3. Brancher les boutons
        vraiButton.onClick.AddListener(() => Repondre(true));
        fauxButton.onClick.AddListener(() => Repondre(false));

        // 4. Préparer la première série de questions
        GenererQuestionsPourMetier();
        AfficherQuestionCourante();
    }

    void InitQuestions()
    {
        // Vendeurs
        questionsVendeur.Add(new QuestionVF {
            texte = "L’IA peut analyser automatiquement les besoins d’un client en boutique.",
            reponse = true
        });
        questionsVendeur.Add(new QuestionVF {
            texte = "L’IA remplace totalement les vendeurs en boutique.",
            reponse = false
        });
        questionsVendeur.Add(new QuestionVF {
            texte = "L’IA peut proposer des équipements adaptés à partir du profil client.",
            reponse = true
        });
        questionsVendeur.Add(new QuestionVF {
            texte = "Utiliser l’IA pour répondre plus vite est interdit en boutique.",
            reponse = false
        });

        // Techniciens
        questionsTechnicien.Add(new QuestionVF {
            texte = "L’IA peut aider à prévoir des pannes réseau avant qu’elles n’arrivent.",
            reponse = true
        });
        questionsTechnicien.Add(new QuestionVF {
            texte = "Les techniciens n’ont aucun intérêt à utiliser l’IA sur le terrain.",
            reponse = false
        });

        // Managers
        questionsManager.Add(new QuestionVF {
            texte = "L’IA peut assister un manager dans l’analyse des performances d’équipe.",
            reponse = true
        });
        questionsManager.Add(new QuestionVF {
            texte = "L’IA décide seule des sanctions disciplinaires.",
            reponse = false
        });
    }

    void GenererQuestionsPourMetier()
    {
        string job = jobDropdown.options[jobDropdown.value].text;
        questionsCourantes.Clear();

        List<QuestionVF> source = job switch
        {
            "Technicien" => questionsTechnicien,
            "Manager" => questionsManager,
            _ => questionsVendeur
        };

        // On prend les N premières pour l’instant
        for (int i = 0; i < Mathf.Min(questionsParJour, source.Count); i++)
            questionsCourantes.Add(source[i]);

        indexQuestion = 0;
        score = 0;
        feedbackText.text = "";
        scoreText.text = "Score : 0";
    }

    void AfficherQuestionCourante()
    {
        if (indexQuestion < questionsCourantes.Count)
        {
            questionText.text = questionsCourantes[indexQuestion].texte;
        }
        else
        {
            questionText.text = "Session terminée.\nScore : " + score + " / " + questionsCourantes.Count * 10;
            feedbackText.text = "Reviens demain pour de nouvelles questions.";
        }
    }

    void Repondre(bool reponseUtilisateur)
    {
        if (indexQuestion >= questionsCourantes.Count)
            return;

        QuestionVF q = questionsCourantes[indexQuestion];
        bool juste = (reponseUtilisateur == q.reponse);

        if (juste)
        {
            score += 10;
            feedbackText.text = "Bonne réponse. L’IA est un outil qui te fait gagner du temps et en précision.";
        }
        else
        {
            string bonne = q.reponse ? "VRAI" : "FAUX";
            feedbackText.text = "Mauvaise réponse. La bonne réponse était : " + bonne +
                                ". L’idée est de comprendre comment l’IA t’aide sans te remplacer.";
        }

        scoreText.text = "Score : " + score;
        indexQuestion++;
        AfficherQuestionCourante();
    }

    // Appelle cette méthode si tu veux relancer une session après changement de métier
    public void OnMetierChange()
    {
        GenererQuestionsPourMetier();
        AfficherQuestionCourante();
    }
}
