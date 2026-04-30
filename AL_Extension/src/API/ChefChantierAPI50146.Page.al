page 50146 "ChefChantierAPI"
{
    PageType = API;
    Caption = 'chefChantierApi';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'chefChantier';
    EntitySetName = 'chefsChantier';
    SourceTable = "Chef Chantier";
    DelayedInsert = true;
    ODataKeyFields = Id;
    // Lecture seule depuis l'API .NET — la gestion des chefs de chantier se fait dans BC
    InsertAllowed = false;
    ModifyAllowed = false;
    DeleteAllowed = false;

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                // --- Identifiants ---
                field(id; Rec.Id)
                {
                    Caption = 'Id';
                    Editable = false;
                }

                // --- Informations personnelles ---
                field(nomEtPrenom; Rec."Nom et Prenom")
                {
                    Caption = 'Nom et Prénom';
                    Editable = false;
                }
                field(email; Rec."Adresse Email")
                {
                    Caption = 'Adresse e-mail';
                    Editable = false;
                }

                // --- Statut & Projet ---
                // Le flag actif est utilisé côté backend pour bloquer les connexions des comptes désactivés
                field(actif; Rec.Actif)
                {
                    Caption = 'Compte actif';
                    Editable = false;
                }
                field(numProjet; Rec."Num Projet")
                {
                    Caption = 'N° Projet géré';
                    Editable = false;
                }

                // --- Approbation ---
                field(idApprobateur; Rec."Id Approbateur")
                {
                    Caption = 'Id Approbateur';
                    Editable = false;
                }
            }
        }
    }
}