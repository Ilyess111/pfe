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
                // --- Identifiants ---
                field(id; Rec.SystemId)
                {
                    Caption = 'Id';
                    Editable = false;
                }
                field(no; Rec."No.")
                {
                    Caption = 'N° Demande';
                    Editable = false;
                }

                // --- Projet ---
                field(jobNo; Rec."Job No.")
                {
                    Caption = 'N° Projet';
                    Editable = false; // Forcé côté backend depuis le JWT — non modifiable par l'API
                }
                field(jobDescription; Rec."Job Description")
                {
                    Caption = 'Libellé Projet';
                    Editable = false;
                }

                // --- Informations générales ---
                field(observation; Rec.Observation)
                {
                    Caption = 'Observation';
                }
                field(requesterId; Rec."Requester ID")
                {
                    Caption = 'Demandeur';
                }
                field(requestType; Rec."Request Type")
                {
                    Caption = 'Type de demande';
                }
                field(service; Rec.Service)
                {
                    Caption = 'Service';
                }

                // --- Engin ---
                field(engin; Rec.Engin)
                {
                    Caption = 'Code Engin';
                }
                field(descriptionEngin; Rec."Description Engin")
                {
                    Caption = 'Désignation Engin';
                    Editable = false;
                }

                // --- Logistique ---
                field(locationCode; Rec."Location Code")
                {
                    Caption = 'Code Magasin';
                }

                // --- Dates ---
                field(orderDate; Rec."Order Date")
                {
                    Caption = 'Date de commande';
                }
                field(dueDate; Rec."Due Date")
                {
                    Caption = 'Date d''échéance';
                }

                // --- Statut & Montant (lecture seule depuis l'API) ---
                field(statut; Rec.Statut)
                {
                    Caption = 'Statut';
                    // La modification du statut passe exclusivement par l'action /submit
                    Editable = false;
                }
                field(amount; Rec.Amount)
                {
                    Caption = 'Montant total';
                    Editable = false;
                }
            }

            part(purchaseRequestLines; "PurchaseRequestLineAPI")
            {
                Caption = 'Lignes';
                EntityName = 'purchaseRequestLine';
                EntitySetName = 'purchaseRequestLines';
                SubPageLink = "Document No." = field("No.");
            }
        }
    }
}