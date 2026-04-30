page 50148 "APIVehiculePointageHeader"
{
    PageType = API;
    Caption = 'vehiculePointageHeader';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'vehiculePointageHeader';
    EntitySetName = 'vehiculePointageHeaders';
    SourceTable = "Entete Pointage Vehicule";
    DelayedInsert = true;
    ODataKeyFields = SystemId;
    InsertAllowed = true;
    ModifyAllowed = true;
    DeleteAllowed = true;

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
                field(documentNo; Rec."N° Document")
                {
                    Caption = 'N° Document';
                    Editable = false;
                }

                // --- Chantier ---
                // jobNo est le pivot de filtrage et de sécurité côté backend (.NET)
                field(jobNo; Rec.Marche)
                {
                    Caption = 'N° Chantier';
                    Editable = false;
                }

                // --- Informations pointage ---
                field(date; Rec.Journee)
                {
                    Caption = 'Date journée';
                }

                // --- Statut ---
                // La modification du statut passe exclusivement par l'action /valider
                field(status; Rec.Statut)
                {
                    Caption = 'Statut';
                    Editable = false;
                }
            }

            part(vehiculePointageLines; "APIVehiculePointageLines")
            {
                Caption = 'Lignes';
                EntityName = 'vehiculePointageLine';
                EntitySetName = 'vehiculePointageLines';
                SubPageLink = "Document N°" = field("N° Document");
            }
        }
    }
}