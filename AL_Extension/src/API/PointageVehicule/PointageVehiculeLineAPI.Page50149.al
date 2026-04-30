page 50149 "APIVehiculePointageLines"
{
    PageType = API;
    Caption = 'vehiculePointageLines';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'vehiculePointageLine';
    EntitySetName = 'vehiculePointageLines';
    SourceTable = "Ligne Pointage Vehicule";
    DelayedInsert = true;
    ODataKeyFields = SystemId;
    InsertAllowed = false;
    ModifyAllowed = true;
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
                field(documentNo; Rec."Document N°")
                {
                    Caption = 'N° Document';
                    Editable = false;
                }

                // --- Véhicule ---
                field(vehiculeNo; Rec.Vehicule)
                {
                    Caption = 'N° Véhicule';
                }
                field(description; Rec.Description)
                {
                    Caption = 'Description';
                    Editable = false;
                }

                // --- Statut ---
                // La modification du statut (ex: Panne) est saisie via ce champ
                field(status; Rec.Statut)
                {
                    Caption = 'Statut';
                }

                // --- Données de travail ---
                field(hoursWorked; Rec."Heure Travailler")
                {
                    Caption = 'Heures travaillées';
                }

                // --- Index (kilométrique ou horaire selon le type de véhicule) ---
                field(startIndex; Rec."Index Depart")
                {
                    Caption = 'Index départ';
                }
                field(endIndex; Rec."Index Final")
                {
                    Caption = 'Index final';
                }

                // --- Consommation ---
                field(fuelConsumed; Rec.Gasoil)
                {
                    Caption = 'Gasoil consommé (L)';
                }

                // --- Maintenance ---
                field(breakdownMotiv; Rec."Motif Panne")
                {
                    Caption = 'Motif de panne';
                }

                // --- Chantier ---
                // marche correspond au jobNo de la ligne — utilisé par AlertService pour la sécurité
                field(marche; Rec.Marche)
                {
                    Caption = 'N° Chantier';
                    Editable = false;
                }
            }
        }
    }
}