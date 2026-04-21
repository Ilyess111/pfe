codeunit 52048897 "PurchaseRequestStatusMgt"
{
    procedure SubmitForApproval(var PurchaseRequest: Record "Purchase Request")
    begin
        PurchaseRequest.TestField(Statut, PurchaseRequest.Statut::Open);
        PurchaseRequest.SuspendStatusCheck(true);
        PurchaseRequest.Statut := PurchaseRequest.Statut::"To Approve";
        if not PurchaseRequest.Modify(true) then begin
            PurchaseRequest.SuspendStatusCheck(false);  // Toujours réinitialiser
            Error('Échec de la mise à jour du statut.');
        end;
        PurchaseRequest.SuspendStatusCheck(false);
    end;
}