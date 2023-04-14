﻿using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeetourAPI.Services;
using SeetourAPI.Data.Models;
using SeetourAPI.Data.Context;
using System.Text;

namespace SeetourAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureImagesURLController : ControllerBase
    {
        private readonly IAzureBlobStorageService _azureBlobStorage;
        private readonly SeetourContext _context;

        public AzureImagesURLController(IAzureBlobStorageService azureBlobStorage , SeetourContext context)
        {
            _azureBlobStorage = azureBlobStorage;
            _context = context;
        }

        #region Uploading Images
        [HttpPost]
        [Route("UploadImage")]
        public async Task<ActionResult> UploadImage(IFormFile file)
        {
            string url;
            try
            {
                url = await _azureBlobStorage.UploadBlobAsync(file);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
            var imageInfo = new Photo
            {
                
            Url = await _azureBlobStorage.UploadBlobAsync(file)
            };
            
            //Getting decoded URl
            _context.Photos.Add(imageInfo);
                _context.SaveChanges();
            

            // Return a response indicating the image was uploaded successfully
            return Ok();
        }
        #endregion


        #region Downloading Images

        //var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        //var blobClient = containerClient.GetBlobClient(fileName);
        //var response = await blobClient.DownloadAsync();
        //return response.Value.Content;

        #endregion


        #region Delete Image
        [HttpDelete]
        [Route("DeleteImage")]
        public async Task<ActionResult> DeleteImage(string url)
        {
          
            //Get the last segment in the URL
            Uri uri = new Uri(url);
            string lastSegment = uri.Segments.LastOrDefault()!;

            //Encoding The Spaces in the Last segment
            string lastSegmentDecoded = Uri.UnescapeDataString(lastSegment);
           // Console.WriteLine(lastSegmentDecoded);

            //if (deleted)
            //    return Ok();

            return NotFound();
        }
        #endregion

        #region UploadImages
        [HttpPost]
        [Route("UploadImages")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            var blobUrls = await _azureBlobStorage.UploadBlobAsyncImgs(files);

            // Save Urls to database
            List<Photo> photos = new();
            blobUrls.ForEach(async url =>
            {
                var photo = new Photo() { Url = url };
                photos.Add(photo);
                await _context.AddAsync(photo);
            });

            await _context.SaveChangesAsync();

            return Ok(blobUrls);
        }

        #endregion

    }
}
