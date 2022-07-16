using DaJet.Http.DataMappers;
using DaJet.Http.Model;
using DaJet.Metadata;
using DaJet.Metadata.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace DaJet.Http.Controllers
{
    [ApiController][Route("")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public sealed class InfoBaseController : ControllerBase
    {
        private readonly InfoBaseDataMapper _mapper = new();
        private readonly IMetadataService _metadataService;
        public InfoBaseController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }
        [HttpGet("ping")] public ActionResult Ping() { return Ok(); }
        [HttpGet("infobase")] public ActionResult SelectInfoBaseList()
        {
            List<InfoBaseModel> list = _mapper.Select();
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
            string json = JsonSerializer.Serialize(list, options);
            return Content(json);
        }
        [HttpGet("infobase/{name}")] public ActionResult SelectInfoBase([FromRoute] string name)
        {
            InfoBaseModel? entity = _mapper.Select(name);

            if (entity == null)
            {
                return NotFound();
            }

            if (!_metadataService.TryGetInfoBase(name, out InfoBase infoBase, out string error))
            {
                return BadRequest(error);
            }

            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            string json = JsonSerializer.Serialize(infoBase, options);

            return Content(json);
        }
        [HttpPost("infobase")] public ActionResult InsertInfoBase([FromBody] InfoBaseModel entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Name) ||
                string.IsNullOrWhiteSpace(entity.ConnectionString) ||
                !Enum.TryParse(entity.DatabaseProvider, out DatabaseProvider provider))
            {
                return BadRequest();
            }

            if (_mapper.Select(entity.Name) != null || !_mapper.Insert(entity))
            {
                return Conflict();
            }
            
            _metadataService.Add(new InfoBaseOptions()
            {
                Key = entity.Name,
                DatabaseProvider = provider,
                ConnectionString = entity.ConnectionString
            });

            return Created($"infobase/{entity.Name}", entity.Name);
        }
        [HttpPut("infobase")] public ActionResult UpdateInfoBase([FromBody] InfoBaseModel entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Name) ||
                string.IsNullOrWhiteSpace(entity.ConnectionString) ||
                !Enum.TryParse(entity.DatabaseProvider, out DatabaseProvider provider))
            {
                return BadRequest();
            }

            InfoBaseModel record = _mapper.Select(entity.Name)!;

            if (record == null)
            {
                return NotFound();
            }

            if (!_mapper.Update(entity))
            {
                return Conflict();
            }

            _metadataService.Remove(entity.Name);
            _metadataService.Add(new InfoBaseOptions()
            {
                Key = entity.Name,
                DatabaseProvider = provider,
                ConnectionString = entity.ConnectionString
            });

            return Ok();
        }
        [HttpDelete("infobase/{name}")] public ActionResult DeleteInfoBase([FromRoute] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest();
            }

            InfoBaseModel entity = _mapper.Select(name)!;

            if (entity == null)
            {
                return NotFound();
            }

            if (!_mapper.Delete(entity))
            {
                return Conflict();
            }

            _metadataService.Remove(entity.Name);

            return Ok();
        }
    }
}
