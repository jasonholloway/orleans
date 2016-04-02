using Orleans.CodeGeneration;
using Orleans.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Runtime
{ 
        
    internal static class GrainTypeMapper
    {
        
        public static GrainTypeMap BuildMap(IEnumerable<GrainTypeData> grainTypeDatas, bool localTestMode) 
        {
            var map = new GrainTypeMap(localTestMode);
            
            foreach(var typeData in grainTypeDatas) 
            {
                var grainTypeCode = GrainInterfaceUtils.GetGrainClassTypeCode(typeData.Type);
                var grainFullName = TypeUtils.GetFullName(typeData.Type);
                var assemblyName = typeData.Type.Assembly.CodeBase;
                var isGenericGrain = typeData.Type.GetClasses().Any(t => t.IsSpecializationOf(typeof(Grain<>)));
                var placementStrategy = GrainTypeData.GetPlacementStrategy(typeData.Type);
                var registrationStrategy = GrainTypeData.GetMultiClusterRegistrationStrategy(typeData.Type);

                foreach(var iface in typeData.RemoteInterfaceTypes) 
                {
                    map.AddEntry(
                            interfaceId: GrainInterfaceUtils.GetGrainInterfaceId(iface),
                            iface: iface,
                            grainTypeCode: grainTypeCode,
                            grainInterface: TypeUtils.GetRawClassName(TypeUtils.GetFullName(iface)),
                            grainClass: grainFullName,
                            assembly: assemblyName,
                            isGenericGrainClass: isGenericGrain, 
                            placement: placementStrategy,
                            registrationStrategy: registrationStrategy,
                            primaryImplementation: (iface.Name.Substring(1) == typeData.Type.Name)
                            );
                }
                
                if(typeData.IsStatelessWorker) {
                    map.AddToUnorderedList(grainTypeCode);
                }                
            }
            
            return map;
        }
        
    }
}
