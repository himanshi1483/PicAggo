using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Google.Apis.Upload;
using PicAggoAPI.Models;

namespace PicAggoAPI.Controllers
{
    public class EventsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Events
        public IQueryable<Events> GetEvents()
        {
            return db.Events;
        }

        // GET: api/Events/5
        [ResponseType(typeof(Events))]
        public async Task<IHttpActionResult> GetEvents(int id)
        {
            Events events = await db.Events.FindAsync(id);
            if (events == null)
            {
                return NotFound();
            }

            return Ok(events);
        }

        // PUT: api/Events/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEvents(int id, Events events)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != events.Id)
            {
                return BadRequest();
            }

            db.Entry(events).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Events
        [ResponseType(typeof(Events))]
        public async Task<IHttpActionResult> PostEvents(Events events)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Events.Add(events);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = events.Id }, events);
        }

        // DELETE: api/Events/5
        [ResponseType(typeof(Events))]
        public async Task<IHttpActionResult> DeleteEvents(int id)
        {
            Events events = await db.Events.FindAsync(id);
            if (events == null)
            {
                return NotFound();
            }

            db.Events.Remove(events);
            await db.SaveChangesAsync();

            return Ok(events);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EventsExists(int id)
        {
            return db.Events.Count(e => e.Id == id) > 0;
        }

        //
        private readonly DriveService _service;

        public GoogleDrive()
        {
            _service = CreateDriveService();
        }

        private FileInfo uploadingFile;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="maximumTransferRate">Indicates the maximum transfer rate for the upload, in MB/s.</param>
        /// <param name="chunkSizeMb"></param>
        /// <returns>Returns ID of new file.</returns>
        public async Task<string> Upload(FileInfo fileInfo, int maximumTransferRate = 0, int chunkSizeMb = 10)
        {
            var uploadStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);

            uploadingFile = fileInfo;

            var uploadArgs = await GetUploadType(fileInfo);

            switch (uploadArgs.UploadType)
            {
                default:
                    return "";
                case UploadType.Create:

                    var insertRequest = _service.Files.Create(
                        new File
                        {
                            Name = fileInfo.Name,
                            Parents = new List<string>
                            {
                            BackupFolderId
                            }
                        },
                        uploadStream,
                        MimeTypes.GetMimeType(fileInfo.Name)
                    );

                    insertRequest.ChunkSize = chunkSizeMb * 1024 * 1024;
                    insertRequest.ProgressChanged += Upload_ProgressChanged;
                    insertRequest.ResponseReceived += Upload_ResponseReceived;

                    var createFileTask = insertRequest.UploadAsync();
                    await createFileTask.ContinueWith(t =>
                    {
                        uploadStream.Dispose();
                    });

                    return insertRequest.ResponseBody.Id;
                case UploadType.Update:

                    var updateRequest = _service.Files.Update(
                        new File
                        {
                            Name = fileInfo.Name,
                            Parents = new List<string>
                            {
                            BackupFolderId
                            }
                        },
                        uploadArgs.FileId,
                        uploadStream,
                        MimeTypes.GetMimeType(fileInfo.Name)
                    );

                    updateRequest.ChunkSize = chunkSizeMb * 1024 * 1024;
                    updateRequest.ProgressChanged += Upload_ProgressChanged;
                    updateRequest.ResponseReceived += Upload_ResponseReceived;

                    var updateFileTask = updateRequest.UploadAsync();
                    await updateFileTask.ContinueWith(t =>
                    {
                        uploadStream.Dispose();
                    });

                    return updateRequest.ResponseBody.Id;
            }
        }

        public async Task<FileList> GetFiles()
        {
            var files = _service.Files.List();
            files.PageSize = 1000;
            files.Fields = "files(id, name, md5Checksum, mimeType, kind)";
            return await files.ExecuteAsync();
        }

        private async Task<UploadArgs> GetUploadType(FileSystemInfo fileOnNas)
        {
            var files = _service.Files.List();
            files.Q = $"name={fileOnNas.Name.ToDbQuote()}";
            files.Fields = "files(id, name, md5Checksum, size)";
            var result = await files.ExecuteAsync();

            if (result.Files.Count == 0)
            {
                return new UploadArgs { UploadType = Backup.UploadType.Create };
            }
            if (result.Files.Count == 1)
            {
                var fileInDrive = result.Files[0];

                using (var stream = System.IO.File.OpenRead(fileOnNas.FullName))
                {

                    return stream.Length == fileInDrive.Size
                            ? new UploadArgs
                            {
                                UploadType = UploadType.None
                            }
                            : new UploadArgs
                            {
                                UploadType = UploadType.Update,
                                FileId = fileInDrive.Id
                            };
                }

            }

            throw new NotSupportedException();
        }

        public delegate void UploadProgressChanged(IUploadProgress progress);

        public event UploadProgressChanged OnUploadProgressChanged;

        private void Upload_ProgressChanged(IUploadProgress progress)
        {
            //OnUploadProgressChanged.Invoke(progress);
            EventLog.WriteEntry("NAS Drive Backup", progress.Status + $" {uploadingFile.Name} {(progress.BytesSent / 1024 / 1024)} of {(uploadingFile.Length / 1024 / 1024)}MB (" + ((float)progress.BytesSent / (float)uploadingFile.Length).ToString("P2") + $") complete" +
                 $"{(progress.Status == UploadStatus.Failed ? progress.Exception.Message + "\r\n" + progress.Exception.StackTrace : "")}", progress.Status == UploadStatus.Failed ? EventLogEntryType.Error : EventLogEntryType.Information);
        }

        private void Upload_ResponseReceived(File file)
        {
            EventLog.WriteEntry("NAS Drive Backup", file.Name + " was uploaded successfully", EventLogEntryType.Information);
        }

        public async Task Delete(string fileId)
        {
            var deleteRequest = _service.Files.Delete(fileId);
            try
            {
                await deleteRequest.ExecuteAsync();
            }
            catch
            {
                var createPermissions = _service.Permissions.Create(new Permission
                {
                    Type = "anyone",
                    Role = "owner"
                }, fileId);

                createPermissions.TransferOwnership = true;
                await createPermissions.ExecuteAsync();
            }
        }

        /// <summary>The core logic for uploading a stream. It is used by the upload and resume methods.</summary>
        private async Task<IUploadProgress> UploadCoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var callback = new ServerErrorCallback(this))
                {

                    byte[] chunk1 = { };

                    byte[] chunk2 = { };

                    PrepareNextChunkKnownSize(ContentStream, cancellationToken, out chunk1, out int _); // Prepare First Chunk

                    bool isCompleted = false;

                    bool usingChunk1 = true;

                    while (!isCompleted)
                    {
                        var getNextChunkTask = Task.Run(() =>
                        {

                            if (usingChunk1)
                            {
                                PrepareNextChunkKnownSizeCustom(ContentStream, cancellationToken, BytesServerReceived + ChunkSize, out chunk2);
                            }
                            else
                            {
                                PrepareNextChunkKnownSizeCustom(ContentStream, cancellationToken, BytesServerReceived + ChunkSize, out chunk1);
                            }


                        }, cancellationToken);

                        var sendChunkTask = usingChunk1 ? SendNextChunkAsync(chunk1, cancellationToken) : SendNextChunkAsync(chunk2, cancellationToken);

                        await Task.WhenAll(getNextChunkTask, sendChunkTask).ConfigureAwait(false);

                        isCompleted = await sendChunkTask;

                        UpdateProgress(new ResumableUploadProgress(UploadStatus.Uploading, BytesServerReceived));

                        usingChunk1 = !usingChunk1;
                    }

                    UpdateProgress(new ResumableUploadProgress(UploadStatus.Completed, BytesServerReceived));
                }
            }

            catch (TaskCanceledException ex)
            {
                Logger.Error(ex, "MediaUpload[{0}] - Task was canceled", UploadUri);
                UpdateProgress(new ResumableUploadProgress(ex, BytesServerReceived));
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "MediaUpload[{0}] - Exception occurred while uploading media", UploadUri);
                UpdateProgress(new ResumableUploadProgress(ex, BytesServerReceived));
            }

            return Progress;
        }



        /// <summary>Prepares the given request with the next chunk in case the steam length is known.</summary>
        private void PrepareNextChunkKnownSizeCustom(Stream stream, CancellationToken cancellationToken, long bytesSent, out byte[] chunk)
        {
            int chkSize;

            try
            {
                chkSize = (int)Math.Min(StreamLength - bytesSent, (long)ChunkSize);
            }
            catch
            {
                // Because we fetch next chunk and upload at the same time, this can fail when the last chunk
                // has uploaded and it tries to fetch the next chunk which doesn't exist. 
                // In this case, return empty initialized values. 
                chunk = new byte[0];

                return;
            }

            // Stream length is known and it supports seek and position operations.
            // We can change the stream position and read bytes from the last point.
            // If the number of bytes received by the server isn't equal to the amount of bytes the client sent, we 
            // need to change the position of the input stream, otherwise we can continue from the current position.
            if (stream.Position != bytesSent)
            {
                stream.Position = bytesSent;
            }

            chunk = new byte[chkSize];
            int bytesLeft = chkSize;
            int bytesRead = 0;
            while (bytesLeft > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Make sure we only read at most BufferSize bytes at a time.
                int readSize = Math.Min(bytesLeft, BufferSize);
                int len = stream.Read(chunk, bytesRead, readSize);
                if (len == 0)
                {
                    // Presumably the stream lied about its length. Not great, but we still have a chunk to send.
                    break;
                }
                bytesRead += len;
                bytesLeft -= len;
            }

        }
    }
}