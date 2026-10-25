using System;
namespace Outbreak.Core
{
    public enum CityZoning { Fixed, NeighborhoodCenters }
    public static partial class CityGenerator
    {
        private static int ZoneDistance(int a,int b,int columns){return Math.Abs(a%columns-b%columns)+Math.Abs(a/columns-b/columns);}
        private static uint ZoneTie(uint seed,int a,int b)
        {
            uint value=seed^unchecked((uint)(a+1)*0x9E3779B9u)^unchecked((uint)(b+2)*0x85EBCA6Bu);value^=value>>16;value*=0x7FEB352Du;value^=value>>15;return value;
        }
        public static BuildingUse[] PlanZones(uint seed,int columns,int rows,CityZoning zoning=CityZoning.NeighborhoodCenters)
        {
            if(seed==0 || columns<1 || columns>6 || rows<1 || rows>6 || (zoning!=CityZoning.Fixed && zoning!=CityZoning.NeighborhoodCenters))throw new ArgumentException("Invalid city zoning settings.");
            int blocks=columns*rows,shops=blocks>6?2:blocks>2?1:0;
            var uses=new BuildingUse[blocks];for(int i=0;i<blocks;i++)uses[i]=BuildingUse.Home;
            if(zoning==CityZoning.Fixed){uses[0]=BuildingUse.DistributionCenter;if(shops>0)uses[2]=BuildingUse.Store;if(shops>1)uses[6]=BuildingUse.Store;return uses;}
            int[] corners={0,columns-1,(rows-1)*columns,blocks-1};int center=corners[ZoneTie(seed,columns,rows)%4];uses[center]=BuildingUse.DistributionCenter;
            if(shops==0)return uses;
            int first=-1,second=-1,bestWorst=int.MaxValue,bestTotal=int.MaxValue,bestDelivery=int.MaxValue;uint bestTie=uint.MaxValue;
            for(int a=0;a<blocks;a++)
            {
                if(a==center)continue;
                for(int b=shops==1?-1:a+1;b<(shops==1?0:blocks);b++)
                {
                    if(b==center)continue;
                    int worst=0,total=0;
                    for(int home=0;home<blocks;home++)
                    {
                        if(home==center || home==a || home==b)continue;
                        int distance=ZoneDistance(home,a,columns);if(b>=0)distance=Math.Min(distance,ZoneDistance(home,b,columns));
                        worst=Math.Max(worst,distance);total+=distance;
                    }
                    int delivery=ZoneDistance(center,a,columns)+(b>=0?ZoneDistance(center,b,columns):0);uint tie=ZoneTie(seed,a,b);
                    if(worst<bestWorst || (worst==bestWorst && (total<bestTotal || (total==bestTotal && (delivery<bestDelivery || (delivery==bestDelivery && tie<bestTie))))))
                    {first=a;second=b;bestWorst=worst;bestTotal=total;bestDelivery=delivery;bestTie=tie;}
                }
            }
            uses[first]=BuildingUse.Store;if(second>=0)uses[second]=BuildingUse.Store;return uses;
        }
        private static BuildingFacing ZoneFacing(BuildingUse[] uses,int block,int columns,BuildingFacing fallback)
        {
            int totalX=0,totalY=0,count=0;
            for(int home=0;home<uses.Length;home++)
            {
                if(uses[home]!=BuildingUse.Home)continue;
                if(uses[block]==BuildingUse.Store)
                {
                    int nearest=-1,best=int.MaxValue;
                    for(int store=0;store<uses.Length;store++)if(uses[store]==BuildingUse.Store){int distance=ZoneDistance(home,store,columns);if(distance<best){best=distance;nearest=store;}}
                    if(nearest!=block)continue;
                }
                totalX+=home%columns-block%columns;totalY+=home/columns-block/columns;count++;
            }
            if(count==0 || (totalX==0 && totalY==0))return fallback;
            return Math.Abs(totalX)>=Math.Abs(totalY)?(totalX>0?BuildingFacing.East:BuildingFacing.West):(totalY>0?BuildingFacing.South:BuildingFacing.North);
        }
    }
}
