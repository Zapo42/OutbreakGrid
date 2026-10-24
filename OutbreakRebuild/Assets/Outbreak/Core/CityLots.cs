using Outbreak.Contracts;
namespace Outbreak.Core
{
    public struct GeneratedBuilding
    {
        public int Left,Top,Width,Height,DoorX,DoorY,PathEndY;
        public BuildingUse Use;
        public int BoxX { get { return Left+Width-2; } }
        public int BoxY { get { return Top+1; } }
    }
    public sealed class GeneratedDistrict
    {
        public WorldMap Map;
        public GeneratedBuilding[] Buildings;
    }
    public static partial class CityGenerator
    {
        // Geometry only. Scenario setup assigns work, residents and finite inventories separately.
        public static GeneratedDistrict BuildingLots(uint seed,int columns=4,int rows=2)
        {
            WorldMap map=Streets(seed,columns,rows);uint axesState=seed;
            int[] xs=Axes(columns,ref axesState),ys=Axes(rows,ref axesState);
            uint state=seed^0xC17B10C5u;if(state==0)state=1;
            var buildings=new GeneratedBuilding[columns*rows];
            for(int row=0;row<rows;row++)for(int column=0;column<columns;column++)
            {
                int index=row*columns+column;
                GeneratedBuilding building=new GeneratedBuilding{Left=xs[column]+5+(int)(LotRandom(ref state)%3),Top=ys[row]+5+(int)(LotRandom(ref state)%3),Width=7+(int)(LotRandom(ref state)%4),Height=7+(int)(LotRandom(ref state)%4),Use=index==0?BuildingUse.DistributionCenter:index==2 || index==6?BuildingUse.Store:BuildingUse.Home};
                building.DoorX=building.Left+building.Width/2;building.DoorY=building.Top+building.Height-1;building.PathEndY=ys[row+1]-1;
                for(int y=building.Top;y<=building.DoorY;y++)for(int x=building.Left;x<building.Left+building.Width;x++)
                    map.Set(x,y,x==building.Left || x==building.Left+building.Width-1 || y==building.Top || y==building.DoorY?Terrain.Wall:Terrain.Floor);
                map.Set(building.DoorX,building.DoorY,Terrain.DoorClosed);
                for(int y=building.DoorY+1;y<=building.PathEndY;y++)map.Set(building.DoorX,y,Terrain.Sidewalk);
                buildings[index]=building;
            }
            MapDocument document=map.Capture();document.name="Generated building district (seed "+seed+")";document.id="city-lots-1";
            return new GeneratedDistrict{Map=WorldMap.Restore(document),Buildings=buildings};
        }
        private static uint LotRandom(ref uint state){state^=state<<13;state^=state>>17;state^=state<<5;return state;}
    }
}
