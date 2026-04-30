page 50150 "GasoilHeaderAPI"
{
    PageType = API;
    Caption = 'gasoilHeader';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'gasoilHeader';
    EntitySetName = 'gasoilHeaders';
    SourceTable = "Entete Fiche Gasoil";
    ODataKeyFields = SystemId;
    DelayedInsert = true;
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
                field(documentNo; Rec."No.")
                {
                    Caption = 'N° Document';
                    Editable = false;
                }

                // --- Chantier ---
                // jobNo est le pivot de filtrage et de sécurité côté backend (.NET)
                field(jobNo; Rec.Chantier)
                {
                    Caption = 'N° Chantier';
                    Editable = false;
                }

                // --- Informations fiche ---
                field(date; Rec.Journee)
                {
                    Caption = 'Date journée';
                }
                field(fileNo; Rec."N° Fiche")
                {
                    Caption = 'N° Fiche';
                    Editable = false;
                }
                field(locationCode; Rec.Cuve)
                {
                    Caption = 'Code cuve';
                }

                // --- Index cuve ---
                field(startIndex; Rec."Index Depart")
                {
                    Caption = 'Index départ';
                }
                field(endIndex; Rec."Index Final")
                {
                    Caption = 'Index final';
                }

                // --- Statut ---
                // La modification du statut passe exclusivement par l'action /valider
                field(status; Rec.Statut)
                {
                    Caption = 'Statut';
                    Editable = false;
                }

                part(gasoilLines; "GasoilLinesAPI")
                {
                    Caption = 'Lignes';
                    EntityName = 'gasoilLine';
                    EntitySetName = 'gasoilLines';
                    SubPageLink = "Document No." = field("No.");
                }
            }
        }
    }
}