using CloudinaryDotNet;
using CloudinaryDotNet.Actions;


namespace DarV2.Service
{
    public static class FileUpload
    {
        public static async Task<ImageUploadResult> UploadAsync(IFormFile file, Cloudinary _cloudinary)
        {
            var result = new ImageUploadResult();

            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = $"DarV2"
                };

                result = await _cloudinary.UploadAsync(uploadParams);
            }

            return result;
        }

        public static async Task<bool> DeleteImageAsync(string publicId, Cloudinary _cloudinary)
        {
            publicId = ExtractPublicId(publicId);
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);

            return result.Result == "ok";
        }
        public static string ExtractPublicId(string url)
        {
            var fileName = url.Split('/').Last().Split('.').First(); 
            return $"DarV2/{fileName}";
        }


    }
}
