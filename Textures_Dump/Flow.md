

```plantuml

actor Player  
entity "Pawns" as Pawns  
database "Hangar" as Hangar  
entity "Mech Construction Recipe" as Recipe  
entity "Mechs" as Mechs  
database "Storage" as Storage  
entity "Mech Construction Bill Stack" as BillStack  
entity "Workgiver (Scanner)" as Workgiver  
  
Player -> BillStack : Add Recipe  
activate BillStack  
  
BillStack -> Recipe : Convert to Mech Construction Bill  
deactivate BillStack  
activate Recipe  
  
Workgiver -> Pawns : Scan and Assign Job  
activate Workgiver  
activate Pawns  
  
Pawns -> Storage : Fetch Resources  
deactivate Pawns  
activate Storage  
  
Storage -> Hangar : Deliver Resources  
deactivate Storage  
activate Hangar  
  
alt Resources complete  
    Hangar -> Recipe : Construct Mech  
    deactivate Hangar  
    activate Recipe  
  
    Recipe -> Mechs : Spawn Mech  
    deactivate Recipe  
    activate Mechs  
  
    Mechs -> Hangar : Park  
    deactivate Mechs  
else Resources incomplete  
    Workgiver --> Recipe : Wait and Retry  
end

```

```plantuml
class Building_Hangar {  
  +ITab iTab  
  +MechConstructionBillStack billStack  
}  
  
class ITab {  
  -selectRecipe()  
}  
  
class MechConstructionBillStack {  
  -addBill()  
  -getBill()  
}  
  
class MechConstructionBill {  
}  
  
class MechRecipeDef {  
}  
  
class Workgiver_Scanner {  
  -scan()  
  -assignJob()  
}  
  
class Mech {  
  -moveTo()  
}  
  
class Pawn {  
  -haul()  
}  
  
Building_Hangar "1" -- "*" ITab  
Building_Hangar "1" -- "*" MechConstructionBillStack  
Building_Hangar "1" -- "*" Mech : stores
ITab "1" -- "1..*" MechRecipeDef : selects  
MechConstructionBillStack "1" -> "*" MechConstructionBill : contains  
MechConstructionBill "1" -- "1" MechRecipeDef : uses  
Workgiver_Scanner --> "*" MechConstructionBill : checks  
Workgiver_Scanner --> "*" Pawn : assigns job  
Pawn --> "*" Building_Hangar : hauls to  
Pawn --> "*" Mech : hauls to  

```

```plantuml
component Building_Hangar {  
  [ITab]  
  [MechConstructionBillStack]  
}  
  
component [Workgiver_Scanner]  
component [Pawn]  
database [Storage] as Storage  
component [Mech]  
  
[ITab] ..> [MechConstructionBillStack] : adds recipe  
[Workgiver_Scanner] ..> [Pawn] : assigns job  
[Pawn] --> [Storage] : fetches resources  
[Storage] --> Building_Hangar : delivers resources  
[MechConstructionBillStack] -> [Mech] : creates  
[Mech] --> Building_Hangar : parks
```
