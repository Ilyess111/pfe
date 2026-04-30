page 50137 "TransferLineAPI"
{
    PageType = API;
    Caption = 'transferLine';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'transferLine';
    EntitySetName = 'transferLines';
    SourceTable = "Transfer Line";
    ODataKeyFields = SystemId;
    DelayedInsert = true;
    InsertAllowed = false;
    // Seul qtyToReceive est modifiable — la réception partielle ou totale par le chef de chantier
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
                field(documentNo; Rec."Document No.")
                {
                    Caption = 'N° Document';
                    Editable = false;
                }
                field(lineNo; Rec."Line No.")
                {
                    Caption = 'N° Ligne';
                    Editable = false;
                }

                // --- Article ---
                field(itemNo; Rec."Item No.")
                {
                    Caption = 'N° Article';
                    Editable = false;
                }
                field(description; Rec.Description)
                {
                    Caption = 'Description';
                    Editable = false;
                }
                field(descriptionSoroubat; Rec."Description Soroubat")
                {
                    Caption = 'Description Soroubat';
                    Editable = false;
                }

                // --- Quantités ---
                field(quantity; Rec.Quantity)
                {
                    Caption = 'Quantité commandée';
                    Editable = false;
                }
                // Quantité expédiée par le magasinier — lecture seule pour le chef
                field(quantityShipped; Rec."Quantity Shipped")
                {
                    Caption = 'Quantité expédiée';
                    Editable = false;
                }
                // Cumul historique des réceptions — lecture seule
                field(quantityReceived; Rec."Quantity Received")
                {
                    Caption = 'Quantité déjà reçue';
                    Editable = false;
                }
                // Seul champ modifiable par le chef de chantier — saisie de la réception
                field(qtyToReceive; Rec."Qty. to Receive")
                {
                    Caption = 'Quantité à réceptionner';
                }

                // --- Unité & Stock ---
                field(unitOfMeasure; Rec."Unit of Measure Code")
                {
                    Caption = 'Unité de mesure';
                    Editable = false;
                }
                field(stock; Rec.Stock)
                {
                    Caption = 'Stock disponible';
                    Editable = false;
                }

                // --- Logistique & Analytique (champs Soroubat Tab-Ext50166) ---
                field(numVehicule; Rec."N° vehicule")
                {
                    Caption = 'N° Véhicule';
                    Editable = false;
                }
                field(affaire; Rec.Affaire)
                {
                    Caption = 'Affaire / Projet';
                    Editable = false;
                }
            }
        }
    }
}