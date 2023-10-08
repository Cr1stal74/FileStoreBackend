using FileStoreBackend.Data.IO;
using FileStoreBackend.Data.Models;
using FileStoreBackend.Data.Providers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net;

namespace FileStoreBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FilesController : ControllerBase
    {
        private static readonly ConcurrentDictionary<Guid, FileModel> _temporaryLinks = new();
        private readonly FileProvider _fileProvider;
        private readonly PostgresProvider _postgresProvider;

        public FilesController(FileProvider fileProvider, PostgresProvider postgresProvider)
        {
            _fileProvider = fileProvider;
            _postgresProvider = postgresProvider;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllFiles()
        {
            return Ok(await _postgresProvider.GetAllFileModelsAsync());
        }

        [HttpGet("/getlink/{id}")]
        public async Task<IActionResult> GetLinkForDownloading([FromRoute] Guid id)
        {
            var tempId = Guid.NewGuid();
            var file = await _postgresProvider.GetFileModelAsync(id);

            if(file is not null && _temporaryLinks.TryAdd(tempId, file))
            {
                return Ok($"/files/{tempId}");
            }
            return NotFound();
        }

        [HttpGet("/files/{tempId}")]
        public async Task<IActionResult> DownloadFile([FromRoute] Guid tempId)
        {
            if(!_temporaryLinks.TryRemove(tempId, out var file))
            {
                return NotFound();
            }

            if(file.IsInDatabase)
            {
                var cfs = new ChunkFileStream(file, _postgresProvider);
                return File(cfs, "some/content", file.Name);
            }
            else
            {
                var fs = _fileProvider.GetDataStream(file);
                return File(fs, "some/content", file.Name);
            }
        }

        [HttpPost("/files")]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> UploadFile()
        {
            bool shouldBeSavedInDb = Request.Headers["Database-Stored"] == "True" ? true : false;

            var file = new FileModel
            {
                Id = Guid.NewGuid(),
                Name = Request.Headers["File-Name"],
                IsInDatabase = shouldBeSavedInDb
            };

            if (shouldBeSavedInDb)
            {
                await _postgresProvider.AddFileAsync(file, Request.Body);
            }
            else
            {
                await _postgresProvider.AddFileModelAsync(file);
                using var fs = _fileProvider.CreateDataStream(file);
                await Request.Body.CopyToAsync(fs);
                file.IsDone = true;
                await _postgresProvider.UpdateFileModelAsync(file);
            }

            return Ok();
        }
    }
}