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

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
                field(documentNo; Rec."Document No.") { Caption = 'Document No.'; Editable = false; }
                field(lineNo; Rec."Line No.") { Caption = 'Line No.'; }
                field(itemNo; Rec."Item No.") { Caption = 'Item No.'; }
                field(description; Rec.Description) { Caption = 'Description'; }
                field(quantity; Rec.Quantity) { Caption = 'Quantity'; }
                
                field(quantityShipped; Rec."Quantity Shipped") 
                { 
                    Caption = 'Quantity Shipped'; 
                    Editable = false; // Le chef de chantier ne peut pas changer ce qui a été expédié
                }
                field(quantityReceived; Rec."Quantity Received") 
                { 
                    Caption = 'Quantity Received'; 
                    Editable = false; // C'est le cumul historique, lecture seule
                }
                field(qtyToReceive; Rec."Qty. to Receive") 
                { 
                    Caption = 'Qty. to Receive'; 
                    // C'est le champ que le chef de chantier va modifier sur Angular
                }
                field(unitOfMeasure; Rec."Unit of Measure Code") { Caption = 'Unit of Measure'; }

                // Champs issus de Tab-Ext50166
                field(stock; Rec.Stock) { Caption = 'Stock'; Editable = false; } 
                field(numVehicule; Rec."N° vehicule") { Caption = 'N° Véhicule'; } // Ajouté (Logistique)
                field(affaire; Rec.Affaire) { Caption = 'Affaire/Projet'; } // Ajouté (Analytique)
                field(descriptionSoroubat; Rec."Description Soroubat") { Caption = 'Description Soroubat'; }
            }
        }
    }
}