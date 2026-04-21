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
                field(Observation; Rec.Observation) { Caption = 'Observation'; }

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
                field(statut; Rec.Statut) { Caption = 'Statut'; }
                field(amount; Rec.Amount)
                {
                    Caption = 'Montant';
                    Editable = false;
                }
                // Champ additionnel de table utile pour le Web
                field(service; Rec.Service) { Caption = 'Service'; }

                // Champ déclencheur pour la transition de statut Open -> To Approve
                field(submitForApproval; SubmitForApprovalAction)
                {
                    Caption = 'Submit For Approval';

                    trigger OnValidate()
                    var
                        StatusMgt: Codeunit "PurchaseRequestStatusMgt";
                    begin
                        if SubmitForApprovalAction then
                            StatusMgt.SubmitForApproval(Rec);
                    end;
                }
                field(bypassStatusCheck; Rec."Bypass Status Check") 
                { 
                    Caption = 'Bypass Status Check'; 
                    Editable = false; 
                }
            }

            part(purchaseRequestLines; "PurchaseRequestLineAPI")
            {
                Caption = 'Lines';
                EntityName = 'purchaseRequestLine';
                EntitySetName = 'purchaseRequestLines';
                SubPageLink = "Document No." = FIELD("No.");
            }
        }
    }

    var
        SubmitForApprovalAction: Boolean;
}
