using System;
namespace Outbreak.Core
{
    public struct CitySetupSummary
    {
        public int Width,Height,Buildings,Homes,Stores,Humans,Consumers,Meals;
    }
    public static class CitySetup
    {
        public static string ValidationError(uint seed,int columns,int rows)
        {
            if(seed==0)return "Seed must be between 1 and 4294967295.";
            if(columns<2 || columns>6 || rows<2 || rows>6)return "Choose 2 to 6 blocks across and 2 to 6 blocks down.";
            return null;
        }
        public static CitySetupSummary Describe(uint seed,int columns,int rows)
        {
            string error=ValidationError(seed,columns,rows);if(error!=null)throw new ArgumentException(error);
            int width,height;CityGenerator.GridSize(seed,columns,rows,out width,out height);
            int buildings=columns*rows,stores=buildings>6?2:1,homes=buildings-stores-1;
            return new CitySetupSummary{Width=width,Height=height,Buildings=buildings,Homes=homes,Stores=stores,Humans=homes,Consumers=homes-stores-1,Meals=40+4*stores+3*homes};
        }
        public static World Create(uint seed,int columns,int rows)
        {
            string error=ValidationError(seed,columns,rows);if(error!=null)throw new ArgumentException(error);
            return EncounterScenarios.GeneratedBuildingTrial(seed,columns,rows);
        }
    }
    public static partial class CityGenerator
    {
        // Measure the seeded street axes without creating terrain during UI rendering.
        internal static void GridSize(uint seed,int columns,int rows,out int width,out int height)
        {
            uint state=seed;int[] xs=Axes(columns,ref state),ys=Axes(rows,ref state);
            width=xs[xs.Length-1]+5;height=ys[ys.Length-1]+5;
        }
    }
}
