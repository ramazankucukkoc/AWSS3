using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AMAZONS3FİLEUPLOADWEBAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BucketsController : ControllerBase
    {
        private readonly IAmazonS3 _amazonS3;

        public BucketsController(IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
        }

        [HttpPost]
        public async Task<IActionResult>CreateBucketAsync(string bucketName)
        {
            bool bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
            if (bucketExists)
                return BadRequest($"Bucket {bucketName} already exists");
            await _amazonS3.PutBucketAsync(bucketName);
            return Ok($"Bucket {bucketName} created.");

        }
        [HttpGet]
        public async Task<IActionResult> GetAllBucketAsync()
        {
            ListBucketsResponse listBucketsResponse =await _amazonS3.ListBucketsAsync();
            return Ok(listBucketsResponse);
        }
        [HttpDelete("{bucketname}")]
        public async Task<IActionResult> DeleteBucket(string bucketName)
        {
            await _amazonS3.DeleteBucketAsync(bucketName);
            return NoContent();
        }


    }
}
