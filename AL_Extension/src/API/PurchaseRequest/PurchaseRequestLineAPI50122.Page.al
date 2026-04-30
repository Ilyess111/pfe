page 50122 "PurchaseRequestLineAPI"
{
    PageType = API;
    Caption = 'purchaseRequestLineApi';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'purchaseRequestLine';
    EntitySetName = 'purchaseRequestLines';
    SourceTable = "Purchase request Line";
    DelayedInsert = true;
    ODataKeyFields = SystemId;

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
                }
                field(lineNo; Rec."Line No.")
                {
                    Caption = 'N° Ligne';
                    Editable = false;
                }

                // --- Article ---
                field(type; Rec.Type)
                {
                    Caption = 'Type';
                }
                field(no; Rec."No.")
                {
                    Caption = 'N° Article / Compte';
                }
                field(description; Rec.Description)
                {
                    Caption = 'Description';
                }
                field(description2; Rec."Description 2")
                {
                    Caption = 'Description 2';
                }
                field(variantCode; Rec."Variant Code")
                {
                    Caption = 'Code variante';
                }

                // --- Quantité & Unité ---
                field(quantity; Rec.Quantity)
                {
                    Caption = 'Quantité';
                }
                field(unitOfMeasureCode; Rec."Unit of Measure Code")
                {
                    Caption = 'Code unité';
                }

                // --- Localisation ---
                field(locationCode; Rec."Location Code")
                {
                    Caption = 'Code magasin';
                }

                // --- Projet & Engin ---
                field(jobNo; Rec."Job No.")
                {
                    Caption = 'N° Projet';
                    // Forcé côté backend depuis le JWT — la valeur est toujours celle du projet du chef
                }
                field(jobTaskNo; Rec."Job Task No.")
                {
                    Caption = 'N° Tâche';
                }
                field(engin; Rec.Engin)
                {
                    Caption = 'Code Engin';
                }

                // --- Options & Montant ---
                field(transferer; Rec."Transférer")
                {
                    Caption = 'À transférer';
                }
                field(lineAmount; Rec."Line Amount")
                {
                    Caption = 'Montant ligne';
                    Editable = false;
                }
            }
        }
    }
}