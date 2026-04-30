page 50123 "JobAPI"
{
    PageType = API;
    Caption = 'jobApi';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'job';
    EntitySetName = 'jobs';
    SourceTable = Job;
    DelayedInsert = true;
    ODataKeyFields = SystemId;
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
                field(id; Rec.SystemId)
                {
                    Caption = 'Id';
                    Editable = false;
                }
                field(no; Rec."No.")
                {
                    Caption = 'N° Projet';
                    Editable = false;
                }

                // --- Informations générales ---
                field(description; Rec.Description)
                {
                    Caption = 'Description';
                    Editable = false;
                }
                field(status; Rec.Status)
                {
                    Caption = 'Statut';
                    Editable = false;
                }

                // --- Planification ---
                field(startingDate; Rec."Starting Date")
                {
                    Caption = 'Date de début';
                    Editable = false;
                }
                field(endingDate; Rec."Ending Date")
                {
                    Caption = 'Date de fin prévue';
                    Editable = false;
                }

                // --- Responsables ---
                // Personne opérationnelle responsable du chantier sur le terrain
                field(personResponsible; Rec."Person Responsible")
                {
                    Caption = 'Responsable opérationnel';
                    Editable = false;
                }
                // Personne administrative responsable du chantier
                field(projectManager; Rec."Project Manager")
                {
                    Caption = 'Chef de projet';
                    Editable = false;
                }

                // --- Logistique ---
                // Magasin d'approvisionnement principal associé au chantier
                field(affectationMagasin; Rec."Affectation Magasin")
                {
                    Caption = 'Magasin affecté';
                    Editable = false;
                }
            }
        }
    }
}