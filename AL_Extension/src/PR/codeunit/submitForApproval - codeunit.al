codeunit 52048897 "PurchaseRequestStatusMgt"
{
    procedure SubmitForApproval(var PurchaseRequest: Record "Purchase Request")
    begin
        PurchaseRequest.TestField(Statut, PurchaseRequest.Statut::Open);
        
        // Activer le flag directement sur le record → persisté dans la transaction
        PurchaseRequest."Bypass Status Check" := true;
        PurchaseRequest.Statut := PurchaseRequest.Statut::"To Approve";
        PurchaseRequest.Modify(true);
        // Pas besoin de reset → OnModify le fait automatiquement
    end;
}