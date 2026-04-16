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

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                field(id; Rec.Id)
                {
                    Caption = 'Id';
                }
                field(nomEtPrenom; Rec."Nom et Prenom")
                {
                    Caption = 'Nom et Prénom';
                }
                field(email; Rec."Adresse Email")
                {
                    Caption = 'Email';
                }
                field(actif; Rec.Actif)
                {
                    Caption = 'Actif';
                }
                field(numProjet; Rec."Num Projet")
                {
                    Caption = 'N° Projet géré';
                }
            }
        }
    }

    // Génération automatique du GUID si vide lors de l'insertion
    trigger OnInsertRecord(BelowxRec: Boolean): Boolean
    begin
        if IsNullGuid(Rec.Id) then
            Rec.Id := CreateGuid();
    end;
}