using DaJet.Http.DataMappers;
using DaJet.Http.Model;
using DaJet.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace DaJet.Http.Controllers
{
    [ApiController][Route("view")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DatabaseViewController : ControllerBase
    {
        private readonly IFileProvider _fileProvider;
        private readonly IMetadataService _metadataService;
        private readonly InfoBaseDataMapper _mapper = new();
        public DatabaseViewController(IMetadataService metadataService, IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            _metadataService = metadataService;
        }

        [HttpGet("{infobase}")] public ActionResult ScriptViews([FromRoute] string infobase)
        {
            string fileName = "query.sql";

            IFileInfo file = _fileProvider.GetFileInfo(fileName);

            if (!file.Exists)
            {
                return NotFound($"File does not exist: {fileName}");
            }

            byte[] content = System.IO.File.ReadAllBytes(file.PhysicalPath);

            if (content == null || content.Length == 0)
            {
                return NotFound($"Script file is empty: {fileName}");
            }

            try
            {
                System.IO.File.Delete(file.PhysicalPath);
            }
            catch
            {
                // do nothing
            }

            return File(content, "application/sql", fileName);

            //Stream stream = file.CreateReadStream();

            //if (stream == null)
            //{
            //    return NotFound(fileName);
            //}

            //return new FileStreamResult(stream, "application/sql") // application/octet-stream
            //{
            //    FileDownloadName = fileName
            //};
        }
        [HttpGet("{infobase}/{type}")] public ActionResult ScriptViews([FromRoute] string infobase, [FromRoute] string type)
        {
            string fileName = "query.sql";

            IFileInfo file = _fileProvider.GetFileInfo(fileName);

            if (!file.Exists)
            {
                return NotFound($"File does not exist: {fileName}");
            }

            byte[] content = System.IO.File.ReadAllBytes(file.PhysicalPath);

            if (content == null || content.Length == 0)
            {
                return NotFound($"Script file is empty: {fileName}");
            }

            try
            {
                System.IO.File.Delete(file.PhysicalPath);
            }
            catch
            {
                // do nothing
            }

            return File(content, "application/sql", fileName);
        }
        [HttpGet("{infobase}/{type}/{name}")] public ActionResult ScriptViews([FromRoute] string infobase, [FromRoute] string type, [FromRoute] string name)
        {
            string fileName = "query.sql";

            IFileInfo file = _fileProvider.GetFileInfo(fileName);

            if (!file.Exists)
            {
                return NotFound($"File does not exist: {fileName}");
            }

            string content;

            using (StreamReader reader = new(file.PhysicalPath, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }

            try
            {
                System.IO.File.Delete(file.PhysicalPath);
            }
            catch
            {
                // do nothing
            }

            return Content(content, "text/plain", Encoding.UTF8);
        }

        [HttpPost("{infobase}")] public ActionResult CreateViews([FromRoute] string infobase)
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
        [HttpPost("{infobase}/{type}")] public ActionResult CreateViews([FromRoute] string infobase, [FromRoute] string type)
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
        [HttpPost("{infobase}/{type}/{name}")] public ActionResult CreateViews([FromRoute] string infobase, [FromRoute] string type, [FromRoute] string name)
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

        [HttpDelete("{infobase}")] public ActionResult DeleteViews([FromRoute] string infobase)
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
        [HttpDelete("{infobase}/{type}")] public ActionResult DeleteViews([FromRoute] string infobase, [FromRoute] string type)
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
        [HttpDelete("{infobase}/{type}/{name}")] public ActionResult DeleteViews([FromRoute] string infobase, [FromRoute] string type, [FromRoute] string name)
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