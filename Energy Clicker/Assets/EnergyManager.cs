using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Ajout de TextMeshPro
using System.Collections;  // Ajout pour utiliser IEnumerator

public class EnergyManager : MonoBehaviour
{
    public int energy = 0;  // La quantité d'énergie accumulée
    public TextMeshProUGUI energyText; // Texte pour afficher l'énergie (utiliser TextMeshPro)
    public Sprite[] batteryLevels; // Tableaux de différents niveaux de batterie
    public Image batteryImage; // Image actuelle affichée dans Unity
    public GameObject factoryMenu; // Menu des usines
    public Button factoryButton; // Bouton pour afficher le menu des usines
    public Button[] factoryButtons; // Liste des boutons d'usine (chaque type d'usine)
    public Button scrollUpButton; // Bouton de défilement vers le haut
    public Button scrollDownButton; // Bouton de défilement vers le bas
    public RectTransform[] factoryPanels; // Panneaux individuels pour chaque type d'usine

    private float clickTimer = 0f;
    private int clicks = 0;
    private Vector3 originalScale; // Échelle d'origine de la batterie
    private int[] factoryCounts; // Nombre d'usines de chaque type
    private int totalFactoryTypes = 10;
    private int currentFactoryIndex = 0; // Index du premier type d'usine affiché

    private void Start()
    {
        // Stocker l'échelle d'origine de l'image de la batterie
        originalScale = batteryImage.transform.localScale;

        // Initialiser les boutons d'usine et leurs états
        factoryCounts = new int[totalFactoryTypes];
        for (int i = 0; i < factoryButtons.Length; i++)
        {
            if (i == 0)
            {
                factoryButtons[i].interactable = true; // L'usine 1 est déverrouillée dès le début
            }
            else
            {
                factoryButtons[i].interactable = false; // Les autres sont verrouillées
            }
        }

        // Ajouter des listeners pour les boutons du menu
        factoryButton.onClick.AddListener(ToggleFactoryMenu);
        scrollUpButton.onClick.AddListener(ScrollUp);
        scrollDownButton.onClick.AddListener(ScrollDown);

        // Ajouter des listeners pour les boutons d'achat d'usine
        for (int i = 0; i < factoryButtons.Length; i++)
        {
            int index = i; // Capturer l'index pour l'utiliser dans le listener
            factoryButtons[i].onClick.AddListener(() => BuyFactory(index));
        }

        // Lancer la production d'énergie pour chaque type d'usine
        for (int i = 0; i < totalFactoryTypes; i++)
        {
            StartProducingEnergy(i);
        }
    }

    // Méthode pour ajouter de l'énergie
    public void AddEnergy()
    {
        int energyToAdd = GetEnergyPerClick();
        energy += energyToAdd;
        UpdateEnergyText();
        clicks++;
        Debug.Log("Bouton cliqué, énergie : " + energy); // Ajoute cette ligne pour voir si le clic est détecté

        // Animer la réduction de la taille de la batterie
        batteryImage.transform.localScale = originalScale * 0.9f;
        Invoke("ResetBatteryScale", 0.1f); // Revenir à l'échelle normale après 0.1 seconde
    }

    // Réinitialiser la taille de la batterie à son échelle d'origine
    private void ResetBatteryScale()
    {
        batteryImage.transform.localScale = originalScale;
    }

    // Calcul de l'énergie ajoutée par clic en fonction du nombre de clics par seconde
    private int GetEnergyPerClick()
    {
        float clicksPerSecond = clickTimer > 0 ? clicks / clickTimer : 0;

        if (clicksPerSecond >= 10)
        {
            return 32;
        }
        else if (clicksPerSecond >= 8)
        {
            return 16;
        }
        else if (clicksPerSecond >= 6)
        {
            return 8;
        }
        else if (clicksPerSecond >= 4)
        {
            return 4;
        }
        else if (clicksPerSecond >= 2)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    // Mise à jour du texte d'énergie
    void UpdateEnergyText()
    {
        if (energyText != null)
        {
            energyText.text = "Énergie : " + energy.ToString();
        }
    }

    void UpdateBatteryImage()
    {
        // Calculer le nombre de clics par seconde
        float clicksPerSecond = clickTimer > 0 ? clicks / clickTimer : 0;

        // Déterminer l'index de l'image à afficher en fonction des clics par seconde
        int index = Mathf.Clamp(Mathf.FloorToInt(clicksPerSecond / 2), 0, batteryLevels.Length - 1);
        if (index >= 0 && index < batteryLevels.Length)
        {
            batteryImage.sprite = batteryLevels[index];
        }
    }

    private void Update()
    {
        // Mettre à jour le compteur de clics par seconde
        clickTimer += Time.deltaTime;

        if (clickTimer >= 1f) // Si une seconde s'est écoulée
        {
            UpdateBatteryImage();
            clickTimer = 0f;
            clicks = 0;
        }
    }

    // Méthode pour acheter une usine
    public void BuyFactory(int factoryType)
    {
        int factoryCost = 100 * (int)Mathf.Pow(10, factoryType); // Coût de base (usine 1 coûte 100, usine 2 coûte 1000, etc.)
        if (energy >= factoryCost)
        {
            energy -= factoryCost;
            factoryCounts[factoryType]++;
            UpdateEnergyText();
            UpdateFactoryPanelCounts();
            Debug.Log("Usine de type " + factoryType + " achetée, nombre total : " + factoryCounts[factoryType]);

            // Vérifier si le type suivant d'usine doit être déverrouillé
            if (factoryType < totalFactoryTypes - 1 && factoryCounts[factoryType] >= 20)
            {
                factoryButtons[factoryType + 1].interactable = true;
            }
        }
    }

    // Méthode pour afficher/masquer le menu des usines
    public void ToggleFactoryMenu()
    {
        factoryMenu.SetActive(!factoryMenu.activeSelf);
    }

    // Méthode pour faire défiler les types d'usines affichées
    private void ScrollUp()
    {
        if (currentFactoryIndex > 0)
        {
            currentFactoryIndex--;
            UpdateFactoryPanels();
        }
    }

    private void ScrollDown()
    {
        if (currentFactoryIndex < totalFactoryTypes - 2)
        {
            currentFactoryIndex++;
            UpdateFactoryPanels();
        }
    }

    private void UpdateFactoryPanels()
    {
        for (int i = 0; i < factoryPanels.Length; i++)
        {
            int factoryTypeToShow = currentFactoryIndex + i;
            if (factoryTypeToShow < totalFactoryTypes)
            {
                factoryPanels[i].gameObject.SetActive(true);
                // Mettre à jour les informations du panneau de l'usine
                TextMeshProUGUI[] texts = factoryPanels[i].GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in texts)
                {
                    if (text.name == "Nom usine")
                    {
                        text.text = "Usine " + (factoryTypeToShow + 1);
                    }
                    else if (text.name == "Cout")
                    {
                        text.text = "Coût : " + (100 * (int)Mathf.Pow(10, factoryTypeToShow)) + " énergies";
                    }
                    else if (text.name == "Production energy")
                    {
                        text.text = factoryTypeToShow == 0 ? "Produit 1 énergie chaque seconde" : "Produit 120 énergies toutes les 10s";
                    }
                    else if (text.name == "NombrePossedee")
                    {
                        text.text = "x" + factoryCounts[factoryTypeToShow];
                    }
                }
            }
            else
            {
                factoryPanels[i].gameObject.SetActive(false);
            }
        }
    }

    // Méthode pour produire de l'énergie automatiquement avec les usines
    private IEnumerator ProduceEnergyOverTime(int factoryType, int energyPerInterval, float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            energy += factoryCounts[factoryType] * energyPerInterval;
            UpdateEnergyText();
            Debug.Log("Énergie produite par les usines de type " + factoryType + " : " + (factoryCounts[factoryType] * energyPerInterval));
        }
    }

    // Lancer la production d'énergie automatique
    public void StartProducingEnergy(int factoryType)
    {
        int energyPerInterval = factoryType == 0 ? 1 : 120; // Chaque usine 1 produit 1 énergie par seconde, chaque usine 2 produit 120 énergies toutes les 10 secondes
        float interval = factoryType == 0 ? 1f : 10f; // Intervalle de production
        StartCoroutine(ProduceEnergyOverTime(factoryType, energyPerInterval, interval));
    }

    // Mise à jour des textes pour le nombre d'usines possédées
    private void UpdateFactoryPanelCounts()
    {
        for (int i = 0; i < factoryPanels.Length; i++)
        {
            int factoryTypeToShow = currentFactoryIndex + i;
            if (factoryTypeToShow < totalFactoryTypes)
            {
                TextMeshProUGUI[] texts = factoryPanels[i].GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in texts)
                {
                    if (text.name == "NombrePossedee")
                    {
                        text.text = "x" + factoryCounts[factoryTypeToShow];
                    }
                }
            }
        }
    }
}
