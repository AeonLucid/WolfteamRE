using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Wolfteam.Packets.Generator.Util;

internal static class DiagnosticUtil
{
    private static readonly DiagnosticDescriptor DescriptorUnsupportedType = new(
        "PKT1", 
        "title", 
        "Unsupported property type {0}", 
        "PacketGenerator", 
        DiagnosticSeverity.Error, 
        true);

    private static readonly DiagnosticDescriptor DescriptorUnsupportedKind = new(
        "PKT2", 
        "title", 
        "Unsupported property kind {0} with type {1}", 
        "PacketGenerator", 
        DiagnosticSeverity.Error, 
        true);

    private static readonly DiagnosticDescriptor DescriptorInvalidObject = new(
        "PKT3", 
        "title", 
        $"An object was referenced that does not implement {Constants.PacketPayloadNamespace}.{Constants.PacketPayloadName}", 
        "PacketGenerator", 
        DiagnosticSeverity.Error, 
        true);

    private static readonly DiagnosticDescriptor DescriptorExceptionCaught = new(
        "PKT4", 
        "title", 
        "Compilation threw an exception", 
        "PacketGenerator", 
        DiagnosticSeverity.Error, 
        true);

    private static readonly DiagnosticDescriptor DescriptorAttributeFieldMissing = new(
        "PKT5", 
        "title", 
        "Field attribute is missing {0}", 
        "PacketGenerator", 
        DiagnosticSeverity.Error, 
        true);

    public static Diagnostic UnknownType(Location location, string propType)
    {
        return Diagnostic.Create(DescriptorUnsupportedType, location, propType);
    }

    public static Diagnostic UnknownKind(Location location, string propType, SyntaxKind propKind)
    {
        return Diagnostic.Create(DescriptorUnsupportedKind, location, propKind, propType);
    }

    public static Diagnostic InvalidObject(Location location)
    {
        return Diagnostic.Create(DescriptorInvalidObject, location);
    }

    public static Diagnostic ExceptionCaught()
    {
        return Diagnostic.Create(DescriptorExceptionCaught, Location.None);
    }

    public static Diagnostic AttributeFieldMissing(Location location, string fieldName)
    {
        return Diagnostic.Create(DescriptorAttributeFieldMissing, location, fieldName);
    }
}