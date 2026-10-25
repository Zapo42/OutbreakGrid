using System;
namespace Outbreak.Core
{
    public struct CitySetupSummary
    {
        public int Width,Height,Buildings,Homes,Stores,Humans,Consumers,Meals;
    }
    public static class CitySetup
    {
        public static string ValidationError(uint seed,int columns,int rows,int homesPerBlock=1,CityStreetLayout layout=CityStreetLayout.Grid,CityZoning zoning=CityZoning.Fixed)
        {
            if(zoning!=CityZoning.Fixed && zoning!=CityZoning.NeighborhoodCenters)return "Choose Fixed layout or Neighborhood centers zoning.";
            if(layout!=CityStreetLayout.Grid && layout!=CityStreetLayout.Neighborhood && layout!=CityStreetLayout.Staggered)return "Choose Grid, Neighborhood or Staggered streets.";
            if(seed==0)return "Seed must be between 1 and 4294967295.";
            if(columns<2 || columns>10 || rows<2 || rows>10)return "Choose 2 to 10 blocks across and 2 to 10 blocks down.";
            if(homesPerBlock<1 || homesPerBlock>4)return "Choose one to four homes per residential block.";
            int blocks=columns*rows,stores=blocks>6?2:1;
            if((blocks-stores-1)*homesPerBlock+stores+1>ShelterRules.MaxRooms)return "Dense neighborhoods exceed 256 buildings here. Reduce block counts or residential density.";
            return null;
        }
        public static CitySetupSummary Describe(uint seed,int columns,int rows,int homesPerBlock=1,CityStreetLayout layout=CityStreetLayout.Grid,CityZoning zoning=CityZoning.Fixed)
        {
            string error=ValidationError(seed,columns,rows,homesPerBlock,layout,zoning);if(error!=null)throw new ArgumentException(error);
            int width,height;CityGenerator.GridSize(seed,columns,rows,out width,out height,layout);
            int blocks=columns*rows,stores=blocks>6?2:1,homes=(blocks-stores-1)*homesPerBlock,buildings=homes+stores+1;
            return new CitySetupSummary{Width=width,Height=height,Buildings=buildings,Homes=homes,Stores=stores,Humans=homes,Consumers=homes-stores-1,Meals=40+4*stores+3*homes};
        }
        public static World Create(uint seed,int columns,int rows,int homesPerBlock=1,CityStreetLayout layout=CityStreetLayout.Grid,CityZoning zoning=CityZoning.Fixed)
        {
            string error=ValidationError(seed,columns,rows,homesPerBlock,layout,zoning);if(error!=null)throw new ArgumentException(error);
            return EncounterScenarios.GeneratedBuildingTrial(seed,columns,rows,homesPerBlock,layout,zoning);
        }
    }
    public static partial class CityGenerator
    {
        // Measure the seeded street axes without creating terrain during UI rendering.
        internal static void GridSize(uint seed,int columns,int rows,out int width,out int height,CityStreetLayout layout=CityStreetLayout.Grid)
        {
            uint state=seed;int[] xs=Axes(columns,ref state,layout==CityStreetLayout.Staggered),ys=Axes(rows,ref state,layout==CityStreetLayout.Staggered);
            width=xs[xs.Length-1]+5;height=ys[ys.Length-1]+5;
        }
    }
}
