page 50121 "PurchaseRequestAPI"
{
    PageType = API;
    Caption = 'purchaseRequestApi';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'purchaseRequest';
    EntitySetName = 'purchaseRequests';
    SourceTable = "Purchase Request";
    DelayedInsert = true;
    ODataKeyFields = SystemId;

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                // Identifiants techniques
                field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
                field(no; Rec."No.") { Caption = 'N° Demande'; Editable = false; }
                field(Observation;Rec.Observation) { Caption = 'Observation'; } 

                // Champs affichés dans la Page 50321 (General)
                field(jobNo; Rec."Job No.") { Caption = 'N° Projet'; }
                field(jobDescription; Rec."Job Description") { Caption = 'Libellé Projet'; }
                field(requesterId; Rec."Requester ID") { Caption = 'Demandeur'; }
                field(requestType; Rec."Request Type") { Caption = 'Type de demande'; }
                field(engin; Rec.Engin) { Caption = 'Code Engin'; }
                field(descriptionEngin; Rec."Description Engin") { Caption = 'Désignation Engin'; }
                field(locationCode; Rec."Location Code") { Caption = 'Code Magasin'; }
                
                field(orderDate; Rec."Order Date") { Caption = 'Date Commande'; }
                field(dueDate; Rec."Due Date") { Caption = 'Date d''échéance'; }
                field(status; Rec.Status) { Caption = 'Statut'; }
                field(amount; Rec.Amount)
                {
                    Caption = 'Montant';
                    Editable = false;
                }
                // Champ additionnel de table utile pour le Web
                field(service; Rec.Service) { Caption = 'Service'; }


            }
        part(purchaseRequestLines; "PurchaseRequestLineAPI") // ca permet de faire un expand sur les lignes de la demande d'achat directement depuis la requête OData, ce qui est très pratique pour le Web
        {
            Caption = 'Lines';
            EntityName = 'purchaseRequestLine';
            EntitySetName = 'purchaseRequestLines';
            SubPageLink = "Document No." = FIELD("No."); 
            // SubPageLink = "Document Id." = FIELD("No."); 
        }

        }
    }
}