using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SIRIMApi.Models;

namespace SIRIMApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManglaresController : ControllerBase
{
    private readonly IMongoCollection<ManglarRegistroMongo>? _collection;
    private readonly string? _configurationError;

    public ManglaresController(IConfiguration config)
    {
        try
        {
            var connectionString = config["MongoDb:ConnectionString"];
            var databaseName = config["MongoDb:DatabaseName"];
            var collectionName = config["MongoDb:CollectionName"];

            if (string.IsNullOrWhiteSpace(connectionString) ||
                string.IsNullOrWhiteSpace(databaseName) ||
                string.IsNullOrWhiteSpace(collectionName))
            {
                _configurationError = "Falta configuración de MongoDB. Configure MongoDb__ConnectionString, MongoDb__DatabaseName y MongoDb__CollectionName.";
                return;
            }

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<ManglarRegistroMongo>(collectionName);
        }
        catch (Exception ex)
        {
            _configurationError = ex.Message;
        }
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        if (_configurationError != null)
        {
            return StatusCode(500, new
            {
                estado = "api activa, pero MongoDB no está configurado correctamente",
                detalle = _configurationError
            });
        }

        return Ok(new
        {
            estado = "api activa",
            baseDatos = "MongoDB configurado"
        });
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (_collection == null)
            return StatusCode(500, new { mensaje = _configurationError });

        var data = await _collection.Find(x => !x.Eliminado).ToListAsync();

        foreach (var item in data)
        {
            item.ServerId = item.Id;
            item.Sincronizado = true;
        }

        return Ok(data);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] List<ManglarRegistroMongo> registros)
    {
        if (_collection == null)
            return StatusCode(500, new { mensaje = _configurationError });

        if (registros == null)
            return BadRequest(new { mensaje = "El cuerpo de la solicitud está vacío." });

        try
        {
            foreach (var registro in registros)
            {
                if (string.IsNullOrWhiteSpace(registro.LocalId))
                    registro.LocalId = Guid.NewGuid().ToString();

                var existente = await _collection
                    .Find(x => x.LocalId == registro.LocalId)
                    .FirstOrDefaultAsync();

                registro.Sincronizado = true;
                registro.ServerId = existente?.Id;

                if (existente == null)
                {
                    registro.Id = null;
                    await _collection.InsertOneAsync(registro);
                    registro.ServerId = registro.Id;
                }
                else
                {
                    registro.Id = existente.Id;
                    registro.ServerId = existente.Id;

                    await _collection.ReplaceOneAsync(
                        x => x.Id == existente.Id,
                        registro
                    );
                }
            }

            var todos = await _collection.Find(x => !x.Eliminado).ToListAsync();

            foreach (var item in todos)
            {
                item.ServerId = item.Id;
                item.Sincronizado = true;
            }

            return Ok(todos);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                mensaje = "Error al sincronizar con MongoDB",
                detalle = ex.Message,
                interno = ex.InnerException?.Message,
                tipo = ex.GetType().Name
            });
        }
    }
}
