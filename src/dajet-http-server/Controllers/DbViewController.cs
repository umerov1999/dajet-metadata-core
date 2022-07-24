using DaJet.Data;
using DaJet.Http.DataMappers;
using DaJet.Http.Model;
using DaJet.Metadata;
using DaJet.Metadata.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace DaJet.Http.Controllers
{
    [ApiController][Route("view")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DbViewController : ControllerBase
    {
        private const string INFOBASE_IS_NOT_FOUND_ERROR = "InfoBase [{0}] is not found. Try register it with the /md service first.";

        private readonly IFileProvider _fileProvider;
        private readonly IMetadataService _metadataService;
        private readonly InfoBaseDataMapper _mapper = new();
        public DbViewController(IMetadataService metadataService, IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            _metadataService = metadataService;
        }

        [Produces("application/sql")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{infobase}")] public ActionResult ScriptViews([FromRoute] string infobase, [FromQuery] string? schema = null, [FromQuery] bool? codify = null)
        {
            InfoBaseModel? record = _mapper.Select(infobase);

            if (record == null)
            {
                return NotFound(string.Format(INFOBASE_IS_NOT_FOUND_ERROR, infobase));
            }

            string fileName = $"view_{infobase}.sql"; // _{DateTime.Now:yyyy-MM-ddTHH:mm.ss}

            IFileInfo file = _fileProvider.GetFileInfo(fileName);

            if (file.Exists)
            {
                try
                {
                    System.IO.File.Delete(file.PhysicalPath);
                }
                catch (Exception exception)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ExceptionHelper.GetErrorMessage(exception));
                }
            }

            if (!_metadataService.TryGetMetadataCache(infobase, out MetadataCache cache, out string error))
            {
                return BadRequest(error);
            }

            if (!_metadataService.TryGetDbViewGenerator(infobase, out IDbViewGenerator generator, out error))
            {
                return BadRequest(error);
            }

            if (!string.IsNullOrWhiteSpace(schema))
            {
                generator.Options.Schema = schema;
            }

            if (codify.HasValue)
            {
                generator.Options.CodifyViewNames = codify.Value;
            }
            
            generator.Options.OutputFile = file.PhysicalPath;

            if (!generator.TryScriptViews(in cache, out int result, out List<string> errors))
            {
                if (result == 0)
                {
                    JsonSerializerOptions options = new()
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    };

                    string json = JsonSerializer.Serialize(errors, options);

                    return Content(json);
                }
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
        [Produces("application/sql")]
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
        [Produces("application/json")]
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

        [HttpPost("{infobase}")] public ActionResult CreateViews([FromRoute] string infobase, [FromBody] DbViewGeneratorOptions options)
        {
            InfoBaseModel? record = _mapper.Select(infobase);

            if (record == null)
            {
                return NotFound(string.Format(INFOBASE_IS_NOT_FOUND_ERROR, infobase));
            }

            if (!_metadataService.TryGetMetadataCache(infobase, out MetadataCache cache, out string error))
            {
                return BadRequest(error);
            }

            if (!_metadataService.TryGetDbViewGenerator(infobase, out IDbViewGenerator generator, out error))
            {
                return BadRequest(error);
            }

            if (!string.IsNullOrWhiteSpace(options.Schema))
            {
                generator.Options.Schema = options.Schema;
            }

            if (options.CodifyViewNames)
            {
                generator.Options.CodifyViewNames = options.CodifyViewNames;
            }

            if (!generator.TryCreateViews(in cache, out int result, out List<string> errors))
            {
                if (result == 0)
                {
                    string json = JsonSerializer.Serialize(errors,
                        new JsonSerializerOptions()
                        {
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                        });
                    return Content(json);
                }
            }

            return Created($"view/{infobase}", infobase);
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

        [HttpDelete("{infobase}")] public ActionResult DeleteViews([FromRoute] string infobase, [FromBody] DbViewGeneratorOptions options)
        {
            InfoBaseModel? record = _mapper.Select(infobase);

            if (record == null)
            {
                return NotFound(string.Format(INFOBASE_IS_NOT_FOUND_ERROR, infobase));
            }

            //if (!_metadataService.TryGetMetadataCache(infobase, out MetadataCache cache, out string error))
            //{
            //    return BadRequest(error);
            //}

            if (!_metadataService.TryGetDbViewGenerator(infobase, out IDbViewGenerator generator, out string error))
            {
                return BadRequest(error);
            }

            if (!string.IsNullOrWhiteSpace(options.Schema))
            {
                generator.Options.Schema = options.Schema;
            }

            if (options.CodifyViewNames)
            {
                generator.Options.CodifyViewNames = options.CodifyViewNames;
            }

            try
            {
                _ = generator.DropViews();
            }
            catch (Exception exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ExceptionHelper.GetErrorMessage(exception));
            }

            return Ok();
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