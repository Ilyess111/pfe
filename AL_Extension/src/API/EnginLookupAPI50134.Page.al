page 50134 "VehiculeLookupAPI"
{
    PageType = API;
    Caption = 'vehiculeLookupApi';
    APIPublisher = 'soroubat';
    APIGroup = 'lookups';
    APIVersion = 'v1.0';
    EntityName = 'vehicule';
    EntitySetName = 'vehicules';
    SourceTable = "Véhicule"; // Nom exact de ta table
    InsertAllowed = false;
    ModifyAllowed = false;

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                field(code; Rec."N° Vehicule") { } 
                
                field(designation; Rec."Désignation") { }
            }
        }
    }
}