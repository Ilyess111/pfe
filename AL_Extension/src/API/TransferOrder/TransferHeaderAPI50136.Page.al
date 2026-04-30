page 50136 "TransferHeaderAPI"
{
    PageType = API;
    Caption = 'transferHeader';
    APIPublisher = 'soroubat';
    APIGroup = 'siteManagement';
    APIVersion = 'v1.0';
    EntityName = 'transferHeader';
    EntitySetName = 'transferHeaders';
    SourceTable = "Transfer Header";
    ODataKeyFields = SystemId;
    DelayedInsert = true;
    // Le chef de chantier ne crée pas d'ordres de transfert — ils sont créés dans BC par le magasinier
    InsertAllowed = false;
    // Seule la réception (qtyToReceive sur les lignes) est modifiable — pas l'en-tête
    ModifyAllowed = false;
    DeleteAllowed = false;

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
                    Caption = 'N° Transfert';
                    Editable = false;
                }

                // --- Statut & Date ---
                field(status; Rec.Status)
                {
                    Caption = 'Statut';
                    Editable = false;
                }
                field(postingDate; Rec."Posting Date")
                {
                    Caption = 'Date d''expédition';
                    Editable = false;
                }

                // --- Localisation ---
                field(transferFromCode; Rec."Transfer-from Code")
                {
                    Caption = 'Magasin source';
                    Editable = false;
                }
                field(transferToCode; Rec."Transfer-to Code")
                {
                    Caption = 'Magasin destination';
                    Editable = false;
                }
                field(inTransitCode; Rec."In-Transit Code")
                {
                    Caption = 'Code transit';
                    Editable = false;
                }

                // --- Chantiers Soroubat ---
                // chantierDestination est le pivot de filtrage côté backend (.NET)
                field(chantierOrigine; Rec."Chantier Origine")
                {
                    Caption = 'Chantier origine';
                    Editable = false;
                }
                field(chantierDestination; Rec."Chantier Destination")
                {
                    Caption = 'Chantier destination';
                    Editable = false;
                }

                // --- Intervenants ---
                field(idExpediteur; Rec."Id Expediteur")
                {
                    Caption = 'Id Expéditeur';
                    Editable = false;
                }
                field(idReceptionneur; Rec."Id Receptioneur")
                {
                    Caption = 'Id Réceptionneur';
                    Editable = false;
                }

                // --- Références ---
                field(observation; Rec.Observation)
                {
                    Caption = 'Observation';
                    Editable = false;
                }
                field(numMateriel; Rec."N° Materiel")
                {
                    Caption = 'N° Matériel';
                    Editable = false;
                }
                field(numDemandeAchat; Rec."N° Demande Achat")
                {
                    Caption = 'N° Demande d''achat';
                    Editable = false;
                }

                part(transferLines; "TransferLineAPI")
                {
                    Caption = 'Lignes';
                    EntityName = 'transferLine';
                    EntitySetName = 'transferLines';
                    SubPageLink = "Document No." = field("No.");
                }
            }
        }
    }
}