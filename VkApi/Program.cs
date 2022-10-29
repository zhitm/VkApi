using System.Diagnostics;
using Newtonsoft.Json;
 
namespace VkApi;
public class Response
    {
        public VkJson[] response = new VkJson[1];
 
        public class VkJson
        {
            public string id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public bool can_access_closed { get; set; }
            public bool is_closed { get; set; }
            public string bdate { get; set; }
            public string music { get; set; }
            public string books { get; set; }
            public string photo { get; set; }
            public string server { get; set; }
            public string hash { get; set; }
            public string profile_aid { get; set; }
            public string upload_url { get; set; }

        }
    }
 


    class VkApiClient
    {
        private static string getHash(string response)
        {
            var info = JsonConvert.DeserializeObject<Response>(response);
            return info.response[0].hash;
        }
        private static string getServer(string response)
        {
            var info = JsonConvert.DeserializeObject<Response>(response);
            return info.response[0].server;
        }
        private static string getPhoto(string response)
        {
            var info = JsonConvert.DeserializeObject<Response>(response);
            return info.response[0].photo;
        }
        private static string getUploadUrl(string response)
        {
            var info = JsonConvert.DeserializeObject<Response>(response);
            // var str = info.response[0].upload_url;
            return info.response[0].upload_url;
        }
        
        private static async Task VkChangePhoto()
        {
            var form = new MultipartFormDataContent
            {
                { new ByteArrayContent(File.ReadAllBytes("../../../photo.jpg"))
                    , "photo", "photo.jpg" }
            };  
            var client = new HttpClient();
            const int clientId = 7499511;
            const string redirectUri = "https://oauth.vk.com/blank.html";
            Console.WriteLine("Введите ID пользователя, информацию о котором вы хотите получить (целое число): ");
            var userId = 343537675;
            // var userId = int.Parse(Console.ReadLine()); // здесь userId нужен только в формате int
            var authString = $"https://oauth.vk.com/authorize?client_id={clientId}&display=page&redirect_uri={redirectUri}&scope=friends&response_type=token&v=5.131&state=123456";
            authString = authString.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {authString}") { CreateNoWindow = true });
            Console.WriteLine("Введите полученный access token: ");
            string access_token = Console.ReadLine();
            
            //to get link
            var json = await client.GetStringAsync($"https://api.vk.com/method/photos.getOwnerPhotoUploadServer?owner_id={userId}&v=5.131&access_token={access_token}");
            var url = getUploadUrl(json);
            var response = await client.PostAsync(url, form);

            // var response = await client.PostAsync("https://pu.vk.com/c230131/ss2019/upload.php?_query=eyJhY3QiOiJvd25lcl9waG90byIsInNhdmUiOjEsImFwaV93cmFwIjp7InNlcnZlciI6OTk5LCJwaG90byI6IntyZXN1bHR9IiwibWlkIjozNDM1Mzc2NzUsImhhc2giOiI3NDQ4ZjkzZjQwNWRmZDVjNDQ0ZjVkNGFhYmI4NDk0MiIsIm1lc3NhZ2VfY29kZSI6MiwicHJvZmlsZV9haWQiOi02fSwib2lkIjozNDM1Mzc2NzUsIm1pZCI6MzQzNTM3Njc1LCJzZXJ2ZXIiOjIzMDEzMSwiX29yaWdpbiI6Imh0dHBzOlwvXC9hcGkudmsuY29tIiwiX3NpZyI6IjgzY2MyNjZkZjEyZDlhNGU1MmU4YTMxMDYyZWZkZjNhIn0", form);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);
            var photo = getPhoto(responseString);
            var server = getServer(responseString);
            var hash = getHash(responseString);
            var savePhoto = await client.GetStringAsync($"https://api.vk.com/method/photos.saveOwnerPhoto?hash={hash}&server={server}&photos={photo}&v=5.131&access_token={access_token}");
            Console.WriteLine(savePhoto);
        }
        private static async Task Main()
        {
            await VkChangePhoto();
        }
    }

