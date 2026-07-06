using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SIRIMApi.Models;

public class ManglarRegistroMongo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string LocalId { get; set; } = "";
    public string? ServerId { get; set; }

    public string NombreZona { get; set; } = "";
    public string EstadoManglar { get; set; } = "";
    public string Observaciones { get; set; } = "";

    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public string UbicacionUrl { get; set; } = "";

    public string Foto1Path { get; set; } = "";
    public string? Foto2Path { get; set; }

    public DateTime FechaRegistro { get; set; }
    public DateTime UltimaModificacion { get; set; }

    public bool Sincronizado { get; set; }
    public bool Eliminado { get; set; }
}
