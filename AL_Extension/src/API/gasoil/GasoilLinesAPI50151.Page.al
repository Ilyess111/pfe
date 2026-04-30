page 50151 "GasoilLinesAPI"
{
    PageType = API;
    Caption = 'gasoilLine';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'gasoilLine';
    EntitySetName = 'gasoilLines';
    SourceTable = "Ligne Fiche Gasoil";
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
                field(documentNo; Rec."Document No.")
                {
                    Caption = 'N° Document';
                    Editable = false;
                }
                field(lineNo; Rec."Numero Ligne")
                {
                    Caption = 'N° Ligne';
                    Editable = false;
                }

                // --- Véhicule ---
                field(vehicleNo; Rec.Materiel)
                {
                    Caption = 'N° Véhicule / Matériel';
                }
                field(vehiclePlate; Rec."Immatricule Vehicule")
                {
                    Caption = 'Immatriculation';
                    Editable = false;
                }
                field(driver; Rec.Chauffeur)
                {
                    Caption = 'Chauffeur';
                }

                // --- Distribution ---
                field(quantity; Rec."Quantité Gasoil")
                {
                    Caption = 'Quantité gasoil (L)';
                }
                field(time; Rec.Heure)
                {
                    Caption = 'Heure de distribution';
                }

                // --- Index ---
                field(indexType; Rec."Type Index")
                {
                    Caption = 'Type d''index';
                }
                field(hourIndex; Rec."Index Horaire")
                {
                    Caption = 'Index horaire';
                }
                field(kmIndex; Rec."Index Kilometrique")
                {
                    Caption = 'Index kilométrique';
                }

                // --- Projet ---
                // projectNo est le pivot de sécurité côté backend (.NET)
                field(projectNo; Rec.Affaire)
                {
                    Caption = 'N° Projet';
                    Editable = false;
                }
            }
        }
    }
}