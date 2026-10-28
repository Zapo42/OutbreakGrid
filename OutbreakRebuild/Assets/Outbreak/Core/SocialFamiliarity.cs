using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class SocialFamiliarityRules
    {
        public const int Limit=8;
    }
    public sealed partial class World
    {
        private bool FamiliarPerson(ActorState actor,int personId)
        {
            if(!SocialFamiliarityEnabled || actor.familiarPeople==null)return false;
            foreach(var note in actor.familiarPeople){if(note.actorId==personId)return true;if(note.actorId>personId)break;}return false;
        }
        public bool RecognizesPerson(int index,int personId)
        {
            if(index<0 || index>=count)throw new ArgumentOutOfRangeException("index");ActorState actor=actors[index];
            return actor.species==Species.Human && actor.health>0 && FamiliarPerson(actor,personId);
        }
        public FamiliarPersonRecord[] FamiliarPeopleKnownBy(int index)
        {
            if(index<0 || index>=count)throw new ArgumentOutOfRangeException("index");ActorState actor=actors[index];
            if(!SocialFamiliarityEnabled || actor.species!=Species.Human || actor.health<=0 || actor.familiarPeople==null)return new FamiliarPersonRecord[0];
            return (FamiliarPersonRecord[])actor.familiarPeople.Clone(); // Never reveal partners' current locations, occupations or health through memory.
        }
        private void RememberGreeting(ref ActorState actor,int personId)
        {
            if(!SocialFamiliarityEnabled)return;var notes=actor.familiarPeople;int length=notes==null?0:notes.Length;
            for(int i=0;i<length;i++)if(notes[i].actorId==personId){notes[i]=new FamiliarPersonRecord{actorId=personId,lastGreetedTick=Tick};return;}
            int evict=-1;
            if(length==SocialFamiliarityRules.Limit)
            {
                evict=0;for(int i=1;i<length;i++)if(notes[i].lastGreetedTick<notes[evict].lastGreetedTick || notes[i].lastGreetedTick==notes[evict].lastGreetedTick && notes[i].actorId>notes[evict].actorId)evict=i;
            }
            var updated=new FamiliarPersonRecord[evict<0?length+1:length];var current=new FamiliarPersonRecord{actorId=personId,lastGreetedTick=Tick};int at=0;bool inserted=false;
            // Keep canonical identity order; replace the least recently greeted person, with stable ties, only when memory is full.
            for(int i=0;i<length;i++){if(i==evict)continue;if(!inserted && notes[i].actorId>personId){updated[at++]=current;inserted=true;}updated[at++]=notes[i];}
            if(!inserted)updated[at]=current;actor.familiarPeople=updated;
        }
        private void ValidateSocialFamiliarity()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];var notes=actor.familiarPeople;if(notes==null)continue;
                if(!SocialFamiliarityEnabled || actor.species!=Species.Human || actor.health<=0 || notes.Length<1 || notes.Length>SocialFamiliarityRules.Limit)throw new ArgumentException("Invalid familiar people owner or count.");
                int previous=0;foreach(var note in notes)
                {
                    if(note.actorId<=previous || note.actorId==actor.id || note.lastGreetedTick<1 || note.lastGreetedTick>Tick)throw new ArgumentException("Invalid familiar person identity, order or encounter time.");
                    bool found=false;for(int j=0;j<count;j++)if(actors[j].id==note.actorId){found=true;break;}
                    if(!found)throw new ArgumentException("Unknown familiar person identity.");previous=note.actorId;
                }
            }
        }
    }
}
