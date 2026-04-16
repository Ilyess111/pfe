page 50145 "ItemLedgerEntryAPI"
{
    PageType = API;
    Caption = 'itemLedgerEntryApi';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'itemLedgerEntry';
    EntitySetName = 'itemLedgerEntries';
    SourceTable = "Item Ledger Entry";
    DelayedInsert = true;
    InsertAllowed = false;
    ModifyAllowed = false;
    DeleteAllowed = false;

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                field(entryNo; Rec."Entry No.") { }
                field(itemNo; Rec."Item No.") { }
                // Utilisation du FlowField de votre extension 
                field(itemDescription; Rec."Designation Article") { } 
                field(locationCode; Rec."Location Code") { }
                field(quantity; Rec.Quantity) { }
                // Champ pivot pour le filtrage par projet 
                field(jobNo; Rec."Job No.") { } 
                field(postingDate; Rec."Posting Date") { }
            }
        }
    }
}