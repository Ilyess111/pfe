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
    // Page en lecture seule totale — les écritures comptables ne sont jamais modifiées depuis l'API
    DelayedInsert = false;
    InsertAllowed = false;
    ModifyAllowed = false;
    DeleteAllowed = false;

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                // --- Identifiants ---
                field(entryNo; Rec."Entry No.")
                {
                    Caption = 'N° Écriture';
                    Editable = false;
                }

                // --- Article ---
                field(itemNo; Rec."Item No.")
                {
                    Caption = 'N° Article';
                    Editable = false;
                }
                // FlowField de l'extension Soroubat — description de l'article
                field(itemDescription; Rec."Designation Article")
                {
                    Caption = 'Désignation article';
                    Editable = false;
                }

                // --- Localisation ---
                field(locationCode; Rec."Location Code")
                {
                    Caption = 'Code magasin';
                    Editable = false;
                }

                // --- Quantité ---
                field(quantity; Rec.Quantity)
                {
                    Caption = 'Quantité';
                    Editable = false;
                }

                // --- Projet & Date ---
                // jobNo est le pivot de filtrage côté backend (.NET)
                field(jobNo; Rec."Job No.")
                {
                    Caption = 'N° Projet';
                    Editable = false;
                }
                // postingDate est utilisé pour calculer LastPostingDate (dernier mouvement) côté backend
                field(postingDate; Rec."Posting Date")
                {
                    Caption = 'Date comptabilisation';
                    Editable = false;
                }
            }
        }
    }
}