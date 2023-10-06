using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.EntityProperties
{
    public enum PropertiesEnum
    {

        //Genral
        Name = 0,
        Description = 1,
        BrickClass,
        Info,
        Value,
        Timestamp,
        ValueQuality,

        //behavior
        Behaviors,
        PollRate,

        BehaviorFunction,
        BehaviorEnable,
        BehaviorActive,
        BehaviorRunnable,
        Running,
        Insight,
        Resolution,
        ExecutionReturnCode,
        ExecutionExceptionMessage,
        LastExecutionStart,
        LastExecutionEnd,
        
        Errors,
        HasError,
        BehaviorValues,


        //analytics
        Conformance,
        ConformanceHistory,
        AverageConformance,
        AverageConformanceHistory,
        Deviation,
        Weight,

        

        Aggregate = 300,

        //area
        Area = 400,
        GrossArea,
        NetArea,
        PanelArea,

        Azimuth,

        BuildingPrimaryFunction,

        //Conversion Efficiency
        ConversionEfficiency = 500,
        MeasuredModuleConversionEfficiency,
        RatedModuleConversionEfficiency,

        CoolingCapacity,

        //Cordinates
        Coordinates = 600,
        LatitudeCoordinates,
        LongitudeCoordinates,
        ElevationCoordinates,

        CurrentFlowType = 700,
        RatedCurrentInput,
        RatedMaximumCurrentInput,
        RatedMinimumCurrentInput,
        RatedCurrentOutput,
        RatedMaximumCurrentOutput,
        RatedMinimumCurrentOutput,

        Deprecation,

        ElectricalPhaseCount,
        ElectricalPhases,
        ElectricalPhaseA,
        ElectricalPhaseB,
        ElectricalPhaseC,

        IsVirtualMeter,

        LastKnownValue,

        MeasuredPowerInput,
        MeasuredPowerOutput,

        OperationalStage,
        OperationalStageCount,

        PowerComplexity,
        PowerFlow,
        RatedPowerInput,
        RatedPowerOutput,
        RatedVoltageInput,
        RatedVoltageOutput,

        TemperatureCoefficientOfPmax,

        ThermalTransmittance,
        BuildingThermalTransmittance,

        Tilt,

        Volume,

        YearBuilt,

        Unit,

        //chiller
        COP,
        KWTon,
        Tons,

        //Cooling Tower
        Approach,
        Range,

        //Thermal
        ThermalEfficiency,


        CanHeat,
        CanCool
    }
}
