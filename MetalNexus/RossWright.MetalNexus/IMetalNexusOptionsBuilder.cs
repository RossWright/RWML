using RossWright.MetalNexus.Schemna;

namespace RossWright.MetalNexus;

public interface IMetalNexusOptionsBuilder 
    : IAssemblyScanningOptionsBuilder
{
    void UseCustomEndpointSchema(ICustomEndpointSchema schema);
    void ConfigureEndpointSchema(Action<IEndpointSchemaOptions> config);
    void IncludeServerStackTraceOnExceptions(bool include = true);
    void TreatUnknownExceptionsAsInternalServiceError(bool throwIseByDefault = true);
}