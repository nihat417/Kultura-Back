using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Kultura.Application.Model;

namespace Kultura.Application.Services
{
    public static class UploadFileHelper
    {
        public static class CloudinaryService
        {
            private static Cloudinary _cloudinary;

            static CloudinaryService()
            {
                var cloudinarySettings = new CloudinarySettings
                {
                    CloudName = "der2ssftz",
                    ApiKey = "324859322961975",
                    ApiSecret = "mKot24NAPmar4_CjCmgsy94LVUk"
                };

                Account account = new Account(
                    cloudinarySettings.CloudName,
                    cloudinarySettings.ApiKey,
                    cloudinarySettings.ApiSecret
                );

                _cloudinary = new Cloudinary(account);
            }

            public static async Task<string> UploadFile(IFormFile file, string folderName)
            {
                if (file == null)
                    throw new ArgumentNullException(nameof(file), "File cannot be null");
                Console.WriteLine("\n file null doul");

                if (string.IsNullOrEmpty(folderName))
                    throw new ArgumentNullException(nameof(folderName), "Folder name cannot be null or empty");
                Console.WriteLine("\n foldername de null doul");

                //var uploadParams = new ImageUploadParams()
                //{
                //    File = new FileDescription(file.FileName, file.OpenReadStream()),
                //    Folder = folderName
                //};

                Console.WriteLine("\n imageupload hissesi");
                var uploadResult = new ImageUploadResult();

                if(file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    var uploadparams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                    };

                    uploadResult =await _cloudinary.UploadAsync(uploadparams);

                }

                //if (uploadResult?.SecureUrl == null)
                //{
                //    throw new Exception("Failed to upload image to Cloudinary");
                //}
                Console.WriteLine("\n secreturl null doul");

                return uploadResult.SecureUrl.AbsoluteUri;

            }

            public static async Task DeleteFile(string publicId)
            {
                var deleteParams = new DeletionParams(publicId);
                await Task.Run(() => _cloudinary.Destroy(deleteParams));
            }
        }
    }
}
