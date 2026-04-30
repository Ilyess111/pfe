page 50120 "JobTaskAPI"
{
    PageType = API;
    Caption = 'jobTaskApi';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'jobTask';
    EntitySetName = 'jobTasks';
    SourceTable = "Job Task";
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
                field(jobNo; Rec."Job No.")
                {
                    Caption = 'N° Projet';
                    Editable = false;
                }
                field(taskNo; Rec."Job Task No.")
                {
                    Caption = 'N° Tâche';
                    Editable = false;
                }

                // --- Informations générales ---
                field(description; Rec.Description)
                {
                    Caption = 'Description';
                    Editable = false;
                }
                field(isBlocked; Rec.Blocked)
                {
                    Caption = 'Bloquée';
                    Editable = false;
                }

                // --- Planification (lecture seule — définie dans BC par le chef de projet) ---
                field(dateDebut; Rec."Date Debut")
                {
                    Caption = 'Date de début';
                    Editable = false;
                }
                field(dateFin; Rec."Date Fin")
                {
                    Caption = 'Date de fin';
                    Editable = false;
                }

                // --- Avancement ---
                // Seul champ modifiable par le chef de chantier depuis l'API
                field(progressPct; Rec."Progress %")
                {
                    Caption = 'Avancement saisi (%)';
                }
                // Calculé automatiquement depuis les quantités réalisées — lecture seule
                field(taskProgressPct; Rec."Task Progress %")
                {
                    Caption = 'Avancement calculé (%)';
                    Editable = false;
                }

                // --- Réalisé (FlowFields calculés dans OnAfterGetRecord) ---
                field(quantityShipped; Rec."Quantity Shipped")
                {
                    Caption = 'Quantité réalisée';
                    Editable = false;
                }
                field(usageTotalCost; Rec."Usage (Total Cost)")
                {
                    Caption = 'Coût réel total';
                    Editable = false;
                }

                // --- Budget initial (lecture seule) ---
                field(initialQuantity; Rec."Initial Quantity")
                {
                    Caption = 'Quantité initiale';
                    Editable = false;
                }
                field(initialUoM; Rec."Initial Unit Of Measure")
                {
                    Caption = 'Unité de mesure';
                    Editable = false;
                }
                field(initialAmount; Rec."Initial Amount")
                {
                    Caption = 'Montant initial';
                    Editable = false;
                }
            }
        }
    }

    trigger OnAfterGetRecord()
    begin
        // Calcul des FlowFields à chaque lecture pour obtenir les valeurs à jour
        Rec.CalcFields("Quantity Shipped");
        Rec.CalcFields("Usage (Total Cost)");
    end;
}