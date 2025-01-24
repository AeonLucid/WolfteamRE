namespace Wolfteam.Packets.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class WolfteamFieldAttribute : Attribute
{
    public WolfteamFieldAttribute()
    {
        Version = 0;
        LengthSize = 0;
        BigEndian = false;
        Encoding = FieldEncoding.ASCII;
    }
    
    /// <summary>
    ///     Specify the client version(s) that this field is valid for.
    /// </summary>
    public ClientVersion Version { get; set; }

    /// <summary>
    ///     Specify the length of the field.
    ///     Valid for strings.
    /// </summary>
    public int Length { get; set; }
    
    /// <summary>
    ///     Specify the amount of bytes used to store the length of the field.
    ///     Valid for strings, arrays and lists.
    /// </summary>
    public int LengthSize { get; set; }

    /// <summary>
    ///     Specify the endianness of the field.
    /// </summary>
    public bool BigEndian { get; set; }

    /// <summary>
    ///     Specify the encoding of the field. Valid for strings.
    /// </summary>
    public FieldEncoding Encoding { get; set; }
}