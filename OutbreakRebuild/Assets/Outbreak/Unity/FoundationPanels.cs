using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using UnityEngine;
namespace Outbreak.UnityAdapter
{
    public sealed partial class FoundationApp
    {
        private readonly Dictionary<string,bool> sections=new Dictionary<string,bool>();
        private bool? sectionDefault;
        private string inspectorIdentity="";
        private bool Section(string title,bool initiallyOpen=false)
        {
            bool open;if(!sections.TryGetValue(title,out open)){open=sectionDefault??initiallyOpen;sections[title]=open;}
            if(GUILayout.Button((open?"Hide ":"Show ")+title))Queue(()=>sections[title]=!open);
            return open;
        }
        private void SetAllSections(bool open)
        {
            sectionDefault=open;sections.Clear();showHouseholds=open;showStores=open;showCenters=open;
        }
        private void RevealObservedEvent()
        {
            sections["Selected item"]=true;sections["Vitals & needs"]=true;sections["Combat & contact"]=true;sections["Recent signals"]=true;inspectorScroll=new Vector2();
        }
        private void DrawEncounterSidebar()
        {
            if(GUILayout.Button("Expand all sections"))Queue(()=>SetAllSections(true));
            if(GUILayout.Button("Collapse all sections"))Queue(()=>SetAllSections(false));
            string identity=selected>=0 && selected<world.Count?"actor:"+world.ActorAt(selected).id:selectedStorageBox>0?"box:"+selectedStorageBox:selectedShelf>0?"shelf:"+selectedShelf:selectedPhone>0?"phone:"+selectedPhone:selectedBatCell>=0?"weapon:"+selectedBatCell:"none";
            if(identity!=inspectorIdentity){inspectorIdentity=identity;sections["Selected item"]=true;sections["Vitals & needs"]=true;inspectorScroll=new Vector2();}
            if(Section("Selected item",true))DrawClickedItem();
            if(selected>=0 && selected<world.Count)
            {
                if(Section("Vitals & needs",true))DrawSelectedVitals();
                if(Section("Combat & contact"))DrawSelectedCombat();
                if(Section("Assignments & actions"))DrawSelectedAssignments();
                if(Section("Memories & traits"))DrawSelectedMemory();
                if(Section("Recent signals"))DrawSelectedSignals();
            }
            else if(selectedStorageBox>0 && Section("Assignments & actions",true))DrawSelectedAssignments();
            DrawEncounterTotalsAndEvents();
            if(Section("Food & shelves"))DrawFoodOverviewPanel();
            if(Section("Shelter supplies") && world.HungerEnabled)DrawShelterSupplies();
            DrawHouseholds();DrawStoreControls();DrawDistributionControls();
            if(Section("Placement tools"))DrawPlacementPanel();
            DrawCancelPlacement();
            if(Section("Weapon supplies"))DrawWeaponSupplyPanel();
            if(Section("Map markers & navigation"))DrawMapMarkerPanel();
            if(Section("Technical details"))DrawTechnicalPanel();
        }
        private void DrawClickedItem()
        {
            GUILayout.Label("SELECTED ITEM");
            if(selected>=0 && selected<world.Count)
            {
                ActorState person=world.ActorAt(selected);GUILayout.Label("#"+person.id+" "+person.species+" at "+person.x+", "+person.y);
                int targetX,targetY;Color targetColor;string targetLabel;if(SelectedTarget(out targetX,out targetY,out targetColor,out targetLabel))GUILayout.Label("Current target: "+targetLabel);
                GUILayout.Label("Health: "+person.health+"/100"+(person.species==Species.Human && person.health>0?" | Energy: "+EnergyRules.Remaining(person)+"/100":""));
                if(person.clerkStoreId!=0)GUILayout.Label("Store clerk #"+person.clerkStoreId+(person.clerkPhone!=0?" | "+(person.clerkCallProgress>0?"Calling for supplies":"Walking to phone"):person.clerkNeedsSupplies?" | Needs supply call":person.clerkShelf!=0?" | Stocking shelves":" | Waiting"));
                if(person.clerkStoreId!=0)foreach(SupplyOrderDocument order in world.PendingSupplyOrders())if(order.storeId==person.clerkStoreId)GUILayout.Label("Pending replenishment order #"+order.id);
                if(person.courierOrderId!=0)GUILayout.Label("Fulfilling order #"+person.courierOrderId);
                if(person.restockPhase!=RestockPhase.None)GUILayout.Label("Current supply task: "+person.restockPhase);
                if(person.courierBoxes!=null || person.distributionCenterId!=0)GUILayout.Label("Courier: "+world.CourierScheduleStatus(selected));
                if(person.turnAtTick>world.Tick)GUILayout.Label("Turning in "+((person.turnAtTick-world.Tick)/(double)Rules.TicksPerSecond).ToString("0.0")+"s");
            }
            else if(selectedStorageBox==0 && selectedShelf==0 && selectedPhone==0 && selectedBatCell<0)GUILayout.Label("Click a human, zombie, shelf, crate or phone to inspect it.");
            if(selectedStorageBox>0){StorageBoxDocument box=world.StorageBoxAt((selectedStorageBox-1)%world.Width,(selectedStorageBox-1)/world.Width);if(box!=null){GUILayout.Label("STORAGE BOX at "+box.x+", "+box.y);GUILayout.Label("Inventory: "+StorageRules.Used(box)+"/"+box.capacity+" items");if(world.RestockingEnabled)GUILayout.Label("Restock reserve: "+world.EffectiveFoodReserve(box)+" meals");if(world.ReserveTargetsEnabled)DrawStorageReserveControls(box);if(box.items.Length==0)GUILayout.Label("Empty");foreach(ItemStackDocument stack in box.items)GUILayout.Label(stack.item+": "+stack.quantity);}}
            if (selectedShelf > 0) { ShelfDocument shelf = world.ShelfAt((selectedShelf - 1) % world.Width, (selectedShelf - 1) / world.Width); if (shelf != null) GUILayout.Label("Shelf at " + shelf.x + ", " + shelf.y + "\nStock: " + shelf.food + " meals\n" + (shelf.searched ? "Searched: " + shelf.food + " meals remain" : "Unsearched: contents unknown to humans")); }
            if(selectedPhone>0 && selected<0 && selectedStorageBox==0 && selectedShelf==0)GUILayout.Label("PHONE BLOCK at "+((selectedPhone-1)%world.Width)+", "+((selectedPhone-1)/world.Width)+"\nStore clerks use an adjacent cell to call for supplies.");
            if(selectedPhone>0 && selected<0 && selectedStorageBox==0 && selectedShelf==0)foreach(SupplyOrderDocument order in world.PendingSupplyOrders())if(order.phone==selectedPhone)GUILayout.Label("Order #"+order.id+" | Store #"+order.storeId+"\nRequested: "+order.requestedMeals+" food\nCaller #"+order.requestedBy+" | Awaiting courier");
            DrawGroundBatInspector();
        }
        private void DrawSelectedVitals()
        {
            if(selected>=0 && selected<world.Count)GUILayout.Label("Health: "+world.ActorAt(selected).health+"/100");
            if(world.Encounter && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human){ActorState person=world.ActorAt(selected);GUILayout.Label(person.turnAtTick==0?"Infection: not infected":"Infection: turning in "+(Math.Max(0,person.turnAtTick-world.Tick)/(double)Rules.TicksPerSecond).ToString("0.0")+"s");}
            if(world.ShoveFatigueEnabled && !world.EnergyEnabled && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human)GUILayout.Label("Shove fatigue: "+world.ActorAt(selected).shoveFatigue+"/"+HumanRules.MaxShoveFatigue);
            if(world.RiseEnabled && selected>=0 && selected<world.Count && world.ActorAt(selected).riseUntilTick>world.Tick)GUILayout.Label("Reanimating: getting up ("+((world.ActorAt(selected).riseUntilTick-world.Tick)/(double)Rules.TicksPerSecond).ToString("0.0")+"s)");
            if(world.MaulingEnabled && selected>=0 && selected<world.Count){ActorState inspected=world.ActorAt(selected);GUILayout.Label((inspected.species==Species.Zombie?"Zombie":"Human")+" health: "+inspected.health+"/100");if(inspected.health==0 && inspected.species==Species.Human){GUILayout.Label(inspected.diedFromDeprivation?"Died from hunger and exhaustion":"Mauled to death");if(world.ReanimationEnabled && inspected.turnAtTick>world.Tick)GUILayout.Label("Reanimation in: "+((inspected.turnAtTick-world.Tick)/(double)Rules.TicksPerSecond).ToString("0.0")+"s");}if(inspected.grappleUntilTick>world.Tick)GUILayout.Label("Grappled by zombie #"+inspected.grappledById+" | struggling to escape");if(inspected.turnAtTick>world.Tick)GUILayout.Label("Infection remaining: "+((inspected.turnAtTick-world.Tick)/(double)Rules.TicksPerGameHour).ToString("0.00")+" game hours");}
            if(world.CorneredShoveEnabled && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human && world.ActorAt(selected).nextShoveTick>world.Tick)GUILayout.Label("Defensive shove ready in: "+((world.ActorAt(selected).nextShoveTick-world.Tick)/5.0).ToString("0.0")+"s");
            if(world.SprintEnabled && selected>=0 && selected<world.Count && world.EscapeSprintAvailable(selected))GUILayout.Label("Escape sprint ready: extra steps cost more energy.");
            if(world.EnergyReturnEnabled && selected>=0 && selected<world.Count && world.ReturningWithLowEnergy(selected))GUILayout.Label("Returning to shelter with low energy.");
            if(world.EnergyRiskEnabled && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human)GUILayout.Label("Low-energy food risk: +"+world.FoodEnergyRiskPenalty(selected));
            if(world.FoodRestEnabled && selected>=0 && selected<world.Count && world.PreparingFoodTrip(selected))GUILayout.Label("Preparing food trip: rest to "+FoodRestRules.DepartureEnergy+" energy (hunger cost).");
            if(selected>=0 && selected<world.Count && world.SurvivalDamageEnabled && world.ActorAt(selected).health>0 && world.ActorAt(selected).species==Species.Human && world.ActorAt(selected).hunger==100 && EnergyRules.Remaining(world.ActorAt(selected))==0)GUILayout.Label("Deteriorating: maximum hunger and no energy. Health decreases over time.");
            if(world.EnergyEnabled && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human && world.ActorAt(selected).health>0)GUILayout.Label("Energy: "+EnergyRules.Remaining(world.ActorAt(selected))+"/"+EnergyRules.Maximum);
            if(world.FoodExploration && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human){ActorState explorer=world.ActorAt(selected);GUILayout.Label("Recently investigated rooms: "+(explorer.foodRoomVisits==null?0:explorer.foodRoomVisits.Length)+(explorer.foodExploreRoomId!=0?"\nInvestigating room "+explorer.foodExploreRoomId:""));}
            if (world.HungerEnabled && selected >= 0 && selected < world.Count && world.ActorAt(selected).species == Species.Human && world.ActorAt(selected).health>0) { ActorState person = world.ActorAt(selected); GUILayout.Label("Hunger " + person.hunger + "/100\nFood inventory " + person.carriedFood + "/" + HungerRules.CarryLimit + " meals\nFood-trip risk " + world.FoodTripRisk(selected) + " / tolerance " + HungerRules.RiskBudget(person) + (person.foodUntilTick > world.Tick ? "\nKnown shelf at " + person.foodX + ", " + person.foodY : "") + (person.homeShelterId != 0 ? "\nSupply trip: return to room " + person.homeShelterId : "")); }
        }
        private void DrawSelectedCombat()
        {
            if(world.WeaponDurability && selected>=0 && selected<world.Count){ActorState owner=world.ActorAt(selected);if(owner.meleeWeapon==MeleeWeapon.Bat)GUILayout.Label("Bat condition: "+(MeleeRules.BatHits-owner.batWear)+"/"+MeleeRules.BatHits+" hits remaining");if(weaponMotion[selected].RecentlyBroken(world.Tick))GUILayout.Label("Bat broke");if(weaponMotion[selected].RecentlySwapped(world.Tick))GUILayout.Label("Bat replaced");}
            if(world.BiteEvasion && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human){ActorState defender=world.ActorAt(selected);GUILayout.Label("Bite evasion: agility "+defender.agility+" | stress "+defender.stress);BiteAction outcome=biteFeedback[selected];if(outcome.targetId==defender.id && world.Tick>=outcome.tick && world.Tick-outcome.tick<BiteRules.FeedbackTicks)GUILayout.Label("Last bite attempt: "+(outcome.evaded?"DODGED":outcome.lethal?"MAULED TO DEATH":outcome.mauling?"MAULED":"BITTEN")+"\nZombie #"+outcome.actorId+" | evasion chance "+outcome.dodgeChance+"%");}
            if(world.CombatExperience && selected>=0 && selected<world.Count){ActorState learner=world.ActorAt(selected);if(learner.species==Species.Human)GUILayout.Label((learner.bluntSkill==10?"Blunt practice: maximum skill":"Blunt practice: "+learner.bluntPractice+"/"+MeleeRules.HitsPerLevel(learner.bluntSkill)+" hits")+(learner.meleeWeapon==MeleeWeapon.Bat?"\nBat damage: "+MeleeRules.Damage(learner):""));}
        }
        private void DrawSelectedAssignments()
        {
            if(world.RestockingEnabled && selected>=0 && selected<world.Count){ActorState restocker=world.ActorAt(selected);if(restocker.restockPhase!=RestockPhase.None)GUILayout.Label((restocker.restockSourceBox==0?"Restocking: ":"Supply delivery: ")+restocker.restockPhase+"\nStorage box: "+((restocker.restockBox-1)%world.Width)+", "+((restocker.restockBox-1)/world.Width)+"\nHome room: "+restocker.restockHomeId+(restocker.restockSourceBox==0?"":"\nDonor box: "+((restocker.restockSourceBox-1)%world.Width)+", "+((restocker.restockSourceBox-1)/world.Width)));}
            DrawSelectedDistributionActions();DrawSelectedStoreActions();DrawCourierAssignmentControls();
        }
        private void DrawSelectedMemory()
        {
            if (world.ShelfMemoryEnabled && selected >= 0 && selected < world.Count) { ShelfMemoryRecord[] notes = world.ActorAt(selected).shelfMemory; if (notes != null) { GUILayout.Label("Remembered shelves: " + notes.Length + "/" + ShelfMemoryRules.Limit); foreach (ShelfMemoryRecord note in notes) GUILayout.Label("Shelf " + note.x + ", " + note.y + ": " + (note.food < 0 ? "unsearched" : note.food == 0 ? "remembered empty" : "last seen " + note.food + " meals") + (note.reportedBy != 0 ? "\nTold by human #" + note.reportedBy : "\nPersonally observed") + "\nSource observation " + ((world.Tick - note.observedTick) / 5.0).ToString("0.0") + " seconds ago"); } } // Distinguish remembered contents from current hidden shelf state.
            if(world.BoxMemoryEnabled && selected>=0 && selected<world.Count){StorageMemoryRecord[] notes=world.ActorAt(selected).storageMemory;if(notes!=null){GUILayout.Label("Remembered boxes: "+notes.Length+"/"+StorageMemoryRules.Limit);foreach(StorageMemoryRecord note in notes)GUILayout.Label("Box "+note.x+", "+note.y+": last seen "+note.food+" meals, "+note.space+" free slots\nObserved "+((world.Tick-note.observedTick)/(double)Rules.TicksPerSecond).ToString("0.0")+" seconds ago"+(note.reportedBy==0?"\nPersonally observed":"\nTold by human #"+note.reportedBy));}}
            if (selected >= 0 && selected < world.Count) { ActorState actor = world.ActorAt(selected); if(world.FoodRouteMemory && actor.blockedFoodUntilTick>world.Tick)GUILayout.Label("Blocked food route: " + actor.blockedFoodX + ", " + actor.blockedFoodY + " | retry in " + ((actor.blockedFoodUntilTick-world.Tick)/5.0).ToString("0.0") + "s"); if(world.CarefulSearch && actor.species==Species.Human && actor.shelfSearchProgress>0)GUILayout.Label("Shelf search: " + actor.shelfSearchProgress + "/3 actions at " + actor.foodX + ", " + actor.foodY); GUILayout.Label("#" + actor.id + " " + actor.species + "\nCell " + actor.x + ", " + actor.y + "\nHealth " + (actor.health / 10f).ToString("0.0") + "\nS/P/E/C/I/A/L: " + actor.strength + "/" + actor.perception + "/" + actor.endurance + "/" + actor.charisma + "/" + actor.intelligence + "/" + actor.agility + "/" + actor.luck + "\nBravery " + actor.bravery + (actor.shelterId != 0 ? "\nShelter " + actor.shelterId + ": " + actor.shelterState + "\nDestination " + actor.shelterX + ", " + actor.shelterY : "") + (world.ShelterRecovery && actor.shelterId != 0 ? "\nExit: " + actor.shelterExitPhase + (actor.shelterCalmSinceTick > 0 && actor.shelterExitPhase == ShelterExitPhase.None ? " | calm wait " + (Math.Max(0, ShelterRecoveryRules.CalmTicks - (world.Tick - actor.shelterCalmSinceTick)) / 5.0).ToString("0.0") + " seconds" : actor.stress > 0 ? " | recovering" : "") : "") + (actor.rejectedShelterId != 0 ? "\nRejected room " + actor.rejectedShelterId : "") + (world.HumanReactions && actor.species == Species.Human ? "\nStress " + HumanRules.Level(actor.stress) + " (" + actor.stress + ")\nResponse " + actor.reaction + "\nSupport " + actor.groupSupport + " | Pressure " + actor.threatPressure + "\nCourage margin " + HumanRules.Margin(actor) + (actor.freezeUntilTick > 0 ? "\nFreeze: " + ((actor.freezeUntilTick - world.Tick) / 5.0).ToString("0.0") + " seconds" : "") : "") + (actor.staggerUntilTick > 0 ? "\nStaggered" : "") + (actor.species == Species.Zombie && actor.targetId == 0 ? "\n" + (world.SearchNoise && actor.heardSearchUntilTick>world.Tick && actor.heardSearchUntilTick>=actor.heardWarningUntilTick && actor.heardSearchUntilTick>=actor.heardDoorUntilTick ? (actor.heardSearchKind==3 ? "Activity: investigating running footsteps at " : actor.heardSearchKind==2 ? "Activity: investigating combat at " : actor.heardSearchKind==1 ? "Activity: investigating door movement at " : "Activity: investigating shelf search at ") + actor.heardSearchX + ", " + actor.heardSearchY + " (" + ((actor.heardSearchUntilTick-world.Tick)/5.0).ToString("0.0") + "s)" : world.ZombieWarningInvestigation && actor.heardWarningUntilTick > world.Tick && actor.heardWarningUntilTick >= actor.heardDoorUntilTick ? "Activity: investigating human warning" : world.ZombieSoundInvestigation && actor.heardDoorUntilTick > world.Tick ? "Activity: investigating heard impact" : "Idle: " + (world.ZombieWandering ? "occasional wandering" : "stationary (older rules)")) : "") + (actor.targetId != 0 ? "\nObserved human #" + actor.targetId + " at " + actor.lastSeenX + ", " + actor.lastSeenY + "\nMemory: " + ((actor.searchUntilTick - world.Tick) / 5.0).ToString("0.0") + " seconds" : "") + (actor.turnAtTick > 0 ? "\nInfected: turns in " + ((actor.turnAtTick - world.Tick) / 5.0).ToString("0.0") + " simulation seconds" : "")); } // Inspect actual saved statistics using a detached value record.
        }
        private void DrawSelectedSignals()
        {
            if (world.DoorBracing && selected >= 0 && selected < world.Count) { ActorState defender = world.ActorAt(selected); if (defender.bracedDoor != 0) GUILayout.Label("Bracing door at " + ((defender.bracedDoor - 1) % world.Width) + ", " + ((defender.bracedDoor - 1) / world.Width)); } // Display the actual saved defensive action.
            if (world.HumanWarnings && selected >= 0 && selected < world.Count) { ActorState listener = world.ActorAt(selected); if (listener.heardWarningUntilTick > world.Tick) GUILayout.Label((listener.species == Species.Zombie ? "Heard warning / investigation at " : "Heard human warning at ") + listener.heardWarningX + ", " + listener.heardWarningY + "\nRecent for " + ((listener.heardWarningUntilTick - world.Tick) / 5.0).ToString("0.0") + " seconds"); } // Show the speaker location rather than a concealed zombie.
            if (world.DoorHearing && selected >= 0 && selected < world.Count) { ActorState listener = world.ActorAt(selected); if (listener.heardDoorUntilTick > world.Tick) GUILayout.Label((listener.species == Species.Zombie ? "Heard impact / investigation at " : "Heard door impact at ") + listener.heardDoorX + ", " + listener.heardDoorY + "\nRecent for " + ((listener.heardDoorUntilTick - world.Tick) / 5.0).ToString("0.0") + " seconds"); } // Display a heard location without inventing a visible attacker.
            if (world.DoorBreaking && selected >= 0 && selected < world.Count) { ActorState actor = world.ActorAt(selected); for (int y = actor.y - 1; y <= actor.y + 1; y++) for (int x = actor.x - 1; x <= actor.x + 1; x++) { int health = world.DoorHealthAt(x, y); if (health >= 0) GUILayout.Label("Nearby door " + x + ", " + y + ": " + (health == 0 ? "BROKEN" : health + "/" + DoorRules.MaxHealth + " HP" + (world.DoorIsBraced(x, y) ? " / BRACED" : ""))); } } // Read actual nearby durability without recomputing or mutating pursuit routes.
        }
        private void DrawFoodOverviewPanel()
        {
            if(world.HungerEnabled)
            {
                FoodOverview food=world.FoodSupplies();GUILayout.Label("FOOD OVERVIEW");
                GUILayout.Label("Humans: " + food.Humans + " | Infected: " + food.Infected);
                GUILayout.Label("Hungry (40+): " + food.Hungry + "\nVery hungry (80+): " + food.VeryHungry);
                GUILayout.Label("Food trips: " + food.SeekingFood + " | Searching: " + food.Searching);
                GUILayout.Label("Investigating rooms: " + food.InvestigatingRooms);
                GUILayout.Label("Meals: carried " + food.CarriedMeals + " | shelves " + food.ShelfMeals);
                if(world.StorageEnabled)GUILayout.Label("Storage boxes: "+food.StorageBoxes+" | Stored meals: "+food.StorageMeals);
                if(world.RestockingEnabled)GUILayout.Label("Restocking trips: "+food.RestockingTrips);
                if(world.SupplyDeliveriesEnabled)GUILayout.Label("Box-to-box deliveries: "+food.SupplyDeliveries);
                GUILayout.Label(food.RemainingMeals==0?"Food supply exhausted":"Food remaining: "+food.RemainingMeals+" meals");
                GUILayout.Label("Shelves: stocked " + food.StockedShelves + " | empty " + food.EmptyShelves + "\nUnsearched shelves: " + food.UnsearchedShelves);
                bool overviewEnabled=GUI.enabled;GUI.enabled=overviewEnabled && clock.Paused && world.Count>0;
                if(GUILayout.Button(new GUIContent("Step 1 game hour","Advance 600 ticks using ordinary simulation actions and remain paused.")))Queue(()=>clock.StepTicks(TickClock.MaxManualTicks,StepWorld));
                GUI.enabled=overviewEnabled && food.Hungry>0;
                if(GUILayout.Button(new GUIContent("Next hungry human","Pause and focus the next human at hunger 40+, in stable identity order.")))Queue(FocusNextHungryHuman);
                GUI.enabled=overviewEnabled && food.StockedShelves>0;
                if(GUILayout.Button(new GUIContent("Next stocked shelf","Pause and focus the next shelf with food, including unsearched stock in the map overview.")))Queue(FocusNextStockedShelf);
                GUI.enabled=overviewEnabled;
            }
        }
        private void DrawPlacementPanel()
        {
            if (GUILayout.Button("New movement test")) Queue(() => { SetPaused(true); confirmReset = true; }); // Open the confirmation on the next stable GUI tree.
            if (GUILayout.Button(new GUIContent("Place human", "Pause and place one baseline human on your next valid map click."))) Queue(() => { SetPaused(true); placing = Species.Human; shelfBrush = -1;restBrush=false;boxBrush=false;phoneBrush=false; status = "Click a walkable cell to place one human."; }); // Arm placement after the current control tree finishes.
            if (GUILayout.Button(new GUIContent("Place zombie", "Pause and place one baseline zombie on your next valid map click."))) Queue(() => { SetPaused(true); placing = Species.Zombie; shelfBrush = -1;restBrush=false;boxBrush=false;phoneBrush=false; status = "Click a walkable cell to place one zombie. Encounter rules apply only in Encounter runs."; }); // Keep species identity explicit without inserting controls mid-event.
            if (world.HungerEnabled) { if (GUILayout.Button("Place stocked shelf")) Queue(() => { SetPaused(true); placing = null; shelfBrush = 6; restBrush=false;boxBrush=false;phoneBrush=false; status = "Click a free cell for a shelf containing six meals."; }); if (GUILayout.Button("Place empty shelf")) Queue(() => { SetPaused(true); placing = null; shelfBrush = 0; restBrush=false;boxBrush=false;phoneBrush=false; status = "Click a free cell for an empty searchable shelf."; }); }
            if(world.StorageEnabled)
            {
                if(GUILayout.Button("Place storage box"))Queue(()=>{SetPaused(true);placing=null;shelfBrush=-1;restBrush=false;boxBrush=true;status="Click a free non-door cell for an empty 30-item storage box. Boxes can be placed in any building or outdoors.";});
                bool boxEnabled=GUI.enabled;GUI.enabled=boxEnabled && world.StorageBoxSites().Length>0;if(GUILayout.Button("Next storage box"))Queue(FocusNextStorageBox);GUI.enabled=boxEnabled;
            }
            if(world.SupplyRequestsEnabled && GUILayout.Button("Place phone block"))Queue(()=>{SetPaused(true);placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=true;status="Click a free indoor floor cell for a phone block.";});
            if(world.RestSpotsEnabled){if(GUILayout.Button("Place bed"))Queue(()=>{SetPaused(true);placing=null;shelfBrush=-1;restBrush=true;boxBrush=false;phoneBrush=false;eraseRestSpot=false;status="Click a walkable floor cell inside a shelter.";});if(GUILayout.Button("Remove bed"))Queue(()=>{SetPaused(true);placing=null;shelfBrush=-1;restBrush=true;boxBrush=false;phoneBrush=false;eraseRestSpot=true;status="Click the bed to remove.";});}
        }
        private void DrawCancelPlacement()
        {
            if ((placing.HasValue || restBrush || boxBrush || phoneBrush || shelfBrush >= 0) && GUILayout.Button("Cancel placement")) Queue(() => { placing = null; shelfBrush = -1;restBrush=false;boxBrush=false;phoneBrush=false; status = "Placement canceled."; });
        }
        private void DrawWeaponSupplyPanel()
        {
            if(world.MeleeCombat)
            {
                int groundBats=0,armedHumans=0,retainedBats=0;foreach(WeaponPickupDocument pile in world.WeaponSites())groundBats+=pile.Quantity;for(int i=0;i<world.Count;i++){ActorState carrier=world.ActorAt(i);if(carrier.meleeWeapon==MeleeWeapon.Bat){if(carrier.species==Species.Human)armedHumans++;else retainedBats++;}}
                GUILayout.Label("WEAPON OVERVIEW");GUILayout.Label("Bats on ground: "+groundBats+"\nArmed humans: "+armedHumans+"\nBats retained by zombies: "+retainedBats);
                bool supplyEnabled=GUI.enabled;GUI.enabled=supplyEnabled && groundBats>0;if(GUILayout.Button("Next ground bat"))Queue(FocusNextGroundBat);GUI.enabled=supplyEnabled;
                bool equipmentEnabled=GUI.enabled;GUI.enabled=equipmentEnabled && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human && world.ActorAt(selected).turnAtTick==0 && world.ActorAt(selected).meleeWeapon==MeleeWeapon.None;
                if(GUILayout.Button(new GUIContent("Place bat at feet","Pause and place one ground bat at the selected human's cell. Picking it up costs an action."))){int recipientIndex=selected;World recipientWorld=world;Queue(()=>{if(world!=recipientWorld || recipientIndex<0 || recipientIndex>=world.Count)return;SetPaused(true);ActorState recipient=world.ActorAt(recipientIndex);if(recipient.species!=Species.Human || recipient.turnAtTick!=0 || recipient.meleeWeapon!=MeleeWeapon.None)return;status=world.TryPlaceBat(recipient.x,recipient.y)?"Bat placed. Step ticks so the human can pick it up when safe.":"Bat needs a walkable non-door cell without another pickup.";});}
                GUI.enabled=equipmentEnabled;
                if(selected>=0 && selected<world.Count){ActorState fighter=world.ActorAt(selected);GUILayout.Label("Melee weapon: "+fighter.meleeWeapon+"\nBlunt skill: "+fighter.bluntSkill+"/10"+(fighter.health==0?"\nDEFEATED":fighter.species==Species.Zombie?"\nEquipment inactive after turning":fighter.nextMeleeTick>world.Tick?"\nSwing cooldown: "+(fighter.nextMeleeTick-world.Tick)+" ticks":""));}
            }
        }
        private void DrawMapMarkerPanel()
        {
            if(GUILayout.Button(showTargetMarkers?"Hide target markers":"Show target markers"))Queue(()=>showTargetMarkers=!showTargetMarkers);
            int targetX,targetY;Color targetColor;string targetLabel;
            if(SelectedTarget(out targetX,out targetY,out targetColor,out targetLabel))GUILayout.Label("Selected target: "+targetLabel);
            if(GUILayout.Button(showInfectionMarkers?"Hide infection markers":"Show infection markers"))Queue(()=>showInfectionMarkers=!showInfectionMarkers);
            if(showInfectionMarkers)GUILayout.Label("Infection: orange square = infected human\nTwo pink squares = corpse awaiting reanimation");
            if(showTargetMarkers)GUILayout.Label("Targets: teal = food | blue = shelter\nRed = sight memory | amber = sound");
            GUILayout.Label("MOVEMENT INSPECTOR"); GUILayout.Label("Middle-drag: pan\nWheel: zoom\nClick: inspect a body"); // Explain the few controls that exist in this slice.
            if (world.SheltersEnabled) GUILayout.Label("Doors: light brown = closed | dark = open\nRed tint = damage | grey = broken"); // Make actual door actions visible while the temporary debug tiles remain.
            if (world.Count == 0) GUILayout.Label("TERRAIN ONLY: no bodies loaded. Place test bodies below, then click Run."); // Make the empty-population state prominent beside the map.
        }
        private void DrawTechnicalPanel()
        {
            GUILayout.Label("Sparse chunks: " + world.AllocatedChunks + "\nTick debt: " + clock.DebtSeconds.ToString("0.000") + " s\nMap: " + world.Width + " x " + world.Height); // Show storage and scheduling evidence directly.
        }
        private void DrawEncounterTotalsAndEvents()
        {
            if(!world.Encounter)return;
                int injured=0,infected=0,humans=0,zombies=0,defeated=0,humanDeaths=0,pendingCorpses=0,rising=0;
                for(int i=0;i<world.Count;i++){ActorState person=world.ActorAt(i);if(person.species==Species.Zombie){if(person.health>0){zombies++;if(person.riseUntilTick>world.Tick)rising++;}else defeated++;continue;}if(person.health<=0){humanDeaths++;if(world.ReanimationEnabled && person.turnAtTick>world.Tick)pendingCorpses++;continue;}humans++;if(person.health<100)injured++;if(person.turnAtTick!=0)infected++;}
            if(Section("Encounter summary"))
            {
                GUILayout.Label("ENCOUNTER SUMMARY");GUILayout.Label("Humans remaining: "+humans+"\nInfected (included): "+infected+"\nActive zombies: "+zombies+"\nDefeated zombies: "+defeated);
                if(world.MaulingEnabled)GUILayout.Label("Human corpses: "+humanDeaths);GUILayout.Label("New conversions: "+newConversions);
                if(world.ReanimationEnabled)GUILayout.Label("Pending corpse reanimations: "+pendingCorpses);if(world.RiseEnabled)GUILayout.Label("Zombies getting up: "+rising);
                DrawPopulationHistory();
                GUILayout.Label(world.Count==0?"No actors placed":pendingCorpses>0?(humans==0?"No humans remain; corpses may still reanimate":zombies>0?"Outbreak ongoing":"Corpses may still reanimate"):humans==0?(zombies>0?"No humans remain":"No living actors remain"):zombies>0?"Outbreak ongoing":infected>0?"Infected humans may still turn":"No active zombies or infection");
            }
            if(Section("Event observation"))
            {
                bool turnEnabled=GUI.enabled;GUI.enabled=turnEnabled && SoonestTurnIndex()>=0;if(GUILayout.Button(new GUIContent("Soonest turn","Pause and focus the earliest pending human conversion or corpse reanimation. Equal deadlines use actor identity order.")))Queue(FocusSoonestTurn);GUI.enabled=turnEnabled;
                int pendingTurn=SoonestTurnIndex();
                if(pendingTurn>=0){ActorState next=world.ActorAt(pendingTurn);long remaining=next.turnAtTick-world.Tick;GUILayout.Label((remaining<=5*Rules.TicksPerSecond?"Turning soon: #":"Next turn: #")+next.id+" in "+(remaining/(double)Rules.TicksPerSecond).ToString("0.0")+"s");}
                if(watchNextTurn){GUILayout.Label("Turn watch active: pauses one tick before the next turn");if(GUILayout.Button("Cancel turn watch"))Queue(()=>{watchNextTurn=false;status="Turn watch cancelled; playback continues.";});}
                else {bool watchEnabled=GUI.enabled;GUI.enabled=watchEnabled && pendingTurn>=0;if(GUILayout.Button(new GUIContent("Watch next turn","Run at the current speed and pause one tick before the earliest pending turn. No ticks are skipped.")))Queue(StartTurnWatch);GUI.enabled=watchEnabled;}
                bool fightEnabled=GUI.enabled;GUI.enabled=fightEnabled && NextFightIndex()>=0;if(GUILayout.Button("Next active fight"))Queue(FocusNextFight);GUI.enabled=fightEnabled;
                if(selected>=0 && selected<world.Count){int opponent=world.CloseVisibleZombie(selected);if(opponent>=0){ActorState zombie=world.ActorAt(opponent);GUILayout.Label("Nearby zombie #"+zombie.id+" | Health: "+zombie.health+"/100");}}
                GUILayout.Label("SURVIVOR HEALTH");GUILayout.Label("Injured humans: "+injured+"\nInfected humans: "+infected);
                bool healthEnabled=GUI.enabled;GUI.enabled=healthEnabled && injured>0;if(GUILayout.Button("Next injured human"))Queue(()=>FocusNextHumanInNeed(false));
                GUI.enabled=healthEnabled && infected>0;if(GUILayout.Button("Next infected human"))Queue(()=>FocusNextHumanInNeed(true));GUI.enabled=healthEnabled;
            }
        }
    }
}
