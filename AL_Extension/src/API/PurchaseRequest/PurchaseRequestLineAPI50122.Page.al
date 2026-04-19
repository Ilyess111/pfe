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
            repeater(Control1)
            {
                // Identifiants techniques
                field(id; Rec.SystemId) 
                { 
                    Caption = 'Id';
                    Editable = false; 
                }
                field(documentNo; Rec."Document No.") 
                { 
                    Caption = 'N° Document';
                    Editable = true; 
                }
                field(lineNo; Rec."Line No.") 
                { 
                    Caption = 'N° Ligne';
                    Editable = false; 
                }
                
                // Champs fonctionnels
                field(transferer; Rec."Transférer") 
                { 
                    Caption = 'Transférer';
                }                
                field(type; Rec.Type) 
                { 
                    Caption = 'Type';
                }
                field(no; Rec."No.") 
                { 
                    Caption = 'N° Article/Compte';
                }
                field(description; Rec.Description) 
                { 
                    Caption = 'Description';
                }
                field(description2; Rec."Description 2") 
                { 
                    Caption = 'Description 2';
                }
                field(quantity; Rec.Quantity) 
                { 
                    Caption = 'Quantité';
                }
                field(unitOfMeasureCode; Rec."Unit of Measure Code") 
                { 
                    Caption = 'Code Unité';
                }
                field(locationCode; Rec."Location Code") 
                { 
                    Caption = 'Code Magasin';
                }
                field(variantCode; Rec."Variant Code") 
                { 
                    Caption = 'Code Variante';
                }

                field(jobNo; Rec."Job No.") 
                { 
                    Caption = 'Project No.';
                    Editable = true;
                }
                field(jobTaskNo; Rec."Job Task No.") 
                { 
                    Caption = 'Project Task No.';
                }

                field(engin; Rec.Engin) 
                { 
                    Caption = 'Code Engin';
                }

                field(lineAmount; Rec."Line Amount") 
                { 
                    Caption = 'Montant Ligne';
                    Editable = false; 
                }
            }
        }
    }
}