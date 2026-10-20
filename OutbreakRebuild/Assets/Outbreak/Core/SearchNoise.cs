using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public static class SearchNoiseRules { public const int Radius=4, MemoryTicks=25; }
    public sealed partial class World
    {
        private static void ClearSearchHearing(ref ActorState actor) {actor.heardSearchX=0;actor.heardSearchY=0;actor.heardSearchUntilTick=0;}
        private void EmitSearchNoise(ActorState searcher)
        {
            PrepareSoundReach(searcher.x,searcher.y);int left=searcher.x-8,top=searcher.y-8;
            for(int i=0;i<count;i++)
            {
                ActorState listener=actors[i];if(listener.species!=Species.Zombie)continue;
                int x=listener.x-left,y=listener.y-top;if(x<0 || y<0 || x>=17 || y>=17)continue;int distance=soundDistance[y*17+x];if(distance<0 || distance>SearchNoiseRules.Radius)continue;
                listener.heardSearchX=searcher.x;listener.heardSearchY=searcher.y;listener.heardSearchUntilTick=Tick+SearchNoiseRules.MemoryTicks;actors[i]=listener;
            }
        }
        private int InvestigateSearch(int index,ref ActorState actor)
        {
            if(actor.x==actor.heardSearchX && actor.y==actor.heardSearchY){ClearSearchHearing(ref actor);actors[index]=actor;return 8;}
            return Route(actor,new ActorState{x=actor.heardSearchX,y=actor.heardSearchY},true); // A sound provides a location, never permission to attack a door or track a human.
        }
        private void ValidateSearchHearing()
        {
            for(int i=0;i<count;i++)
            {
                ActorState actor=actors[i];if(actor.heardSearchUntilTick==0){if(actor.heardSearchX!=0 || actor.heardSearchY!=0)throw new ArgumentException("Inactive shelf search hearing contains coordinates.");}
                else if(!SearchNoise || actor.species!=Species.Zombie || actor.heardSearchUntilTick<=Tick || actor.heardSearchUntilTick-Tick>SearchNoiseRules.MemoryTicks || !ValidObservedCell(actor.heardSearchX,actor.heardSearchY))throw new ArgumentException("Invalid shelf search hearing.");
            }
        }
    }
}
