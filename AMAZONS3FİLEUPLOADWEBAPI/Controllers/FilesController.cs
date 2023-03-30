using Amazon.S3;
using Amazon.S3.Model;
using AMAZONS3FİLEUPLOADWEBAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AMAZONS3FİLEUPLOADWEBAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3 _amazonS3;

        public FilesController(IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
        }
        [HttpPost]
        public async Task<IActionResult> UploadAsync(IFormFile formFile, string bucketName, string? prefix)
        {
            bool bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
                return NotFound($"Bucket {bucketName} does not exists");

            PutObjectRequest request = new()
            {
                BucketName = bucketName,
                Key = String.IsNullOrEmpty(prefix) ? formFile.FileName : $"{prefix?.TrimEnd('/')}/{formFile.FileName}",
                InputStream = formFile.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", formFile.ContentType);
            await _amazonS3.PutObjectAsync(request);
            return Ok($"File {prefix}/{formFile.FileName} uploaded to S3 successfully!");

        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(string bucketName, string? prefix)
        {
            bool bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
                return NotFound($"Bucket {bucketName} does not exists");
            ListObjectsV2Request request = new()
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            ListObjectsV2Response response = await _amazonS3.ListObjectsV2Async(request);
            List<S3AWSObject> objectDatas = response.S3Objects.Select(@object =>
            {
                GetPreSignedUrlRequest urlRequest = new()
                {
                    BucketName = bucketName,
                    Key = @object.Key,
                    Expires = DateTime.UtcNow.AddMinutes(1),
                };
                return new S3AWSObject
                {
                    Name = @object.Key,
                    Url = _amazonS3.GetPreSignedURL(urlRequest)
                };
            }).ToList();
            return Ok(objectDatas);
        }

        [HttpDelete("{bucketName}/{fileName}")]
        public async Task<IActionResult>DeleteFileAsync([FromRoute]string bucketName,[FromRoute]string fileName)
        {
            bool bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
                return NotFound($"Bucket {bucketName} does not exists");
            await _amazonS3.DeleteObjectAsync(bucketName, fileName);
            return NoContent();
        }

        [HttpGet("download/{bucketName}/{fileName}")]
        public async Task<IActionResult>GetFileByNameAsync([FromRoute] string bucketName,[FromRoute] string fileName)
        {
            bool bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);

            if (!bucketExists)
                return NotFound($"Bucket {bucketName} does not exist");

            GetObjectResponse response = await _amazonS3.GetObjectAsync(bucketName, fileName);

            return File(response.ResponseStream, response.Headers.ContentType);
        }

    }
}
