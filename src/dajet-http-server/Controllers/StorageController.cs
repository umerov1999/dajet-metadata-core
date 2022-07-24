using DaJet.Http.DataMappers;
using DaJet.Http.Model;
using DaJet.Metadata;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace DaJet.Http.Controllers
{
    [ApiController]
    [Route("storage")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class StorageController : ControllerBase
    {
        private readonly InfoBaseDataMapper _mapper = new();
        private readonly IMetadataService _metadataService;
        public StorageController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }
        [HttpGet("{infobase}")] public ActionResult Select([FromRoute] string infobase)
        {
            InfoBaseModel entity = new()
            {
                Name = infobase
            };

            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            string json = JsonSerializer.Serialize(entity, options);

            return Content(json);
        }
    }
}