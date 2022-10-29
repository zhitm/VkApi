
using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


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
 
 
    public class PhotoOwnerJson
    {
        public string[] response { get; set; }
    }
 
    class VkApiClient
    {
        private static async Task UsersGet()
        {
            var httpClient = new HttpClient();
 
            const string method = "users.get";
            Console.WriteLine("Введите свой access token: ");
            string accessToken = Console.ReadLine();
            Console.WriteLine("Введите ID пользователя, о котором вы хотите получить информацию: ");
            string userId = Console.ReadLine();
 
            var json = await httpClient.GetStringAsync(
                $"https://api.vk.com/method/{method}?user_ids={userId}&fields=music&access_token={accessToken}&v=5.131"
            );
 
            // Console.WriteLine(json);
            var userInfo = JsonConvert.DeserializeObject<Response>(json);
            if (userInfo != null)
            {
                Console.WriteLine(userInfo.response[0].first_name + " " + userInfo.response[0].last_name);
                Console.WriteLine(userInfo.response[0].music);
            }
            else
            {
                Console.WriteLine("Ошибка при чтении данных");
            }
 
            Console.WriteLine("Нажмите <Enter>, чтобы закрыть консоль");
            Console.ReadLine(); // задержка консоли. Актуально только для Windows
        }
 
        private static async Task VkAuthorize()
        {
            var client = new HttpClient();
            const int clientId = 51459598;
            const string redirectUri = "https://oauth.vk.com/blank.html";
            Console.WriteLine("Введите ID пользователя, информацию о котором вы хотите получить (целое число): ");
            var userId = int.Parse(Console.ReadLine()); // здесь userId нужен только в формате int
            var authString = $"https://oauth.vk.com/authorize?client_id={clientId}&display=page&redirect_uri={redirectUri}&scope=friends&response_type=token&v=5.131&state=123456";
            authString = authString.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {authString}") { CreateNoWindow = true });
 
            Console.WriteLine("Введите полученный access token: ");
            var access_token = Console.ReadLine();
            var json = await client.GetStringAsync($"https://api.vk.com/method/friends.getOnline?user_id={userId}&v=5.131&access_token={access_token}");
 
            // Console.WriteLine(json);
            var (startIndex, endIndex) = (json.IndexOf("["), json.IndexOf("]"));
 
            var friendsList = json.Substring(startIndex + 1, endIndex - startIndex - 1).Split(',').Select(prop => int.Parse(prop)).ToList();
            /*foreach (var id in friendsList)
            {
                Console.Write(id.ToString() + " ");
            }
            Console.ReadLine();
            */
 
            foreach (var id in friendsList)
            {
                var userResponse = await client.GetStringAsync($"https://api.vk.com/method/users.get?user_id={id}&v=5.131&fields=bdate&access_token={access_token}");
                var userAccount = JsonConvert.DeserializeObject<Response>(userResponse);
                try
                {
                    Console.WriteLine(userAccount.response[0].first_name + " " + userAccount.response[0].last_name);
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Ошибка чтения данных");
                }
            }
 
            Console.WriteLine("Нажмите <Enter>, чтобы закрыть консоль");
            Console.ReadLine(); // задержка консоли. Актуально только для Windows
        }
 
        
        private static async Task UploadPhoto()
        {
            Console.WriteLine("Введите путь к фото: ");
            var photo_path = Console.ReadLine();
            var startIndex = Math.Max(photo_path.LastIndexOf("/"), photo_path.LastIndexOf("\\"));
            var endIndex = photo_path.LastIndexOf(".");
            var short_file_name = photo_path.Substring(startIndex + 1, endIndex - startIndex - 1);
            var full_file_name = photo_path.Substring(startIndex + 1);
            var form = new MultipartFormDataContent
            {
                { new ByteArrayContent(File.ReadAllBytes(photo_path))
                    , short_file_name, full_file_name}
            };
            var client = new HttpClient();
            const int clientId = 7499511;
            const string redirectUri = "https://oauth.vk.com/blank.html";
            Console.WriteLine("Введите ID пользователя или группы для изменения фото (целое число): ");
            var userId = int.Parse(Console.ReadLine());
            var authString = $"https://oauth.vk.com/authorize?client_id={clientId}&display=page&redirect_uri={redirectUri}&scope=friends&response_type=token&v=5.131&state=123456";
            authString = authString.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {authString}") { CreateNoWindow = true });
            Console.WriteLine("Введите полученный access token: ");
            string access_token = Console.ReadLine();
 
            //to get link
            var json = await client.GetStringAsync($"https://api.vk.com/method/photos.getOwnerPhotoUploadServer?owner_id={userId}&v=5.131&access_token={access_token}");
            // var url = getUploadUrl(json);
            JObject responseWithUrl = JObject.Parse(json);
            var url = responseWithUrl["response"]["upload_url"].ToString();
            Console.WriteLine(url);
            // var url = "https://pu.vk.com/c852032/ss2291/upload.php?_query=eyJhY3QiOiJvd25lcl9waG90byIsInNhdmUiOjEsImFwaV93cmFwIjp7InNlcnZlciI6OTk5LCJwaG90byI6IntyZXN1bHR9IiwibWlkIjozNDM1Mzc2NzUsImhhc2giOiI3NDQ4ZjkzZjQwNWRmZDVjNDQ0ZjVkNGFhYmI4NDk0MiIsIm1lc3NhZ2VfY29kZSI6MiwicHJvZmlsZV9haWQiOi02fSwib2lkIjozNDM1Mzc2NzUsIm1pZCI6MzQzNTM3Njc1LCJzZXJ2ZXIiOjg1MjAzMiwiX29yaWdpbiI6Imh0dHBzOlwvXC9hcGkudmsuY29tIiwiX3NpZyI6ImFmN2I5NmI2OTNkYzE3ZGUyNDY0MjNmMDVlY2U4ODdkIn0";           
            var response = await client.PostAsync(url, form);
 
            var responseString = await response.Content.ReadAsStringAsync();
            // Console.WriteLine(responseString);
            // var photo = getPhoto(responseString);
            // var server = getServer(responseString);
            JObject responseWithPhoto = JObject.Parse(responseString);

            var server = responseWithPhoto["server"].ToString();
            var photo = responseWithPhoto["photo"].ToString();
            // var photo = "eyJvaWQiOjM0MzUzNzY3NSwicGhvdG8iOnsibWFya2Vyc19yZXN0YXJ0ZWQiOnRydWUsInBob3RvIjoiZTQwMDBiM2Q1ODp5Iiwic2l6ZXMiOltdLCJsYXRpdHVkZSI6MCwibG9uZ2l0dWRlIjowLCJraWQiOiI2YWZhYTY0NGVhYjQxOTE1NzY0NzVhOGFhNmVmODU5ZCIsInNpemVzMiI6W1sicyIsIjQ1NTM1MGI3ZDI0NjFlNjliNjU5NzQ4YzkyY2VjOWE3ZDQxOTcxY2QyZDhlZmExZjEyZmMzY2NjIiwiLTcwNzgwNTA2NDEyMzk1ODY3MDEiLDc1LDUyXSxbIm0iLCI5ZDFiNDRjODU5MDVmYThiMWVhY2ViNzU2ZDIxNDQwYTYwYWJhMDI0NmIyMTY5OGVjOGIwNjA4NCIsIi01OTk0MjYyN TQ2NzIzNDc4ODgxIiwxMzAsOTFdLFsieCIsImE2NDYxMjg3YTFlY2QwZGNiZTA4YzA4NThjNjRmNGU2MTAyYTg2ZmNiOGVjOTczODgzM2I0Zjc1IiwiLTM3NjA0NjE0MjA1NDIyODEyNDMiLDYwNCw0MjVdLFsieSIsIjRhMWE5MGM5MjAwYWUwMDNhNzQ0YWJhZjgwNGRkMGVlODM5ZDMxYTdiNmExYjkwNzZiY TlhODc2IiwiLTc2Njg2MDI2NTU2MjU1NTUxMzEiLDgwMCw1NjNdLFsibyIsIjlkMWI0NGM4NTkwNWZhOGIxZWFjZWI3NTZkMjE0NDBhNjBhYmEwMjQ2YjIxNjk4ZWM4YjA2MDg0IiwiLTU5OTQyNjI1NDY3MjM0Nzg4ODEiLDEzMCw5MV0sWyJwIiwiZTE4NTQ1MGUxNDI5M2FlMzhmYmM2MDdiYTFlM2E2ODI3M jA3ZTZkMWJkZjkwZDg5MDU2M2ZhMjIiLCI4MDU4NzI1ODU0MjIwNjkyMzQwIiwyMDAsMTQxXSxbInEiLCJiNzViOGY0NjU0MDI3N2FkODAxOTJjNTMzYzM1OWE4ODNlYjJkOTlkZTJjYTY3OGViMmZiM2VhZiIsIi03NjE1NTYzMjA1MTE4MzMwMjg1IiwzMjAsMjI1XSxbInIiLCJhNzhhMmVhY2FjMzQyOGNmM DkwMmFiNzVlN2NkMDdiZmRlZGJhMWE5NTUxYTA0Nzk0ODdlODk2NCIsIjM2NTMzMjkwODMzODg1ODk3MjIiLDUxMCwzNTldXSwidXJscyI6W10sInVybHMyIjpbIlJWTlF0OUpHSG1tMldYU01rczdKcDlRWmNjMHRqdm9mRXZ3OHpBXC9jMGl3WW5HMnhaMC5qcGciLCJuUnRFeUZrRi1vc2VyT3QxYlNGRUNtQ 3JvQ1JySVdtT3lMQmdoQVwvbjJJZUJQa1owS3cuanBnIiwicGtZU2g2SHMwTnktQ01DRmpHVDA1aEFxaHZ5NDdKYzRnenRQZFFcLzVUa0VCVU1vME1zLmpwZyIsIlNocVF5U0FLNEFPblJLdXZnRTNRN29PZE1hZTJvYmtIYTZtb2RnXC9SVlBTdzRPbWs1VS5qcGciLCJuUnRFeUZrRi1vc2VyT3QxYlNGRUNtQ 3JvQ1JySVdtT3lMQmdoQVwvbjJJZUJQa1owS3cuanBnIiwiNFlWRkRoUXBPdU9QdkdCN29lT21nbklINXRHOS1RMkpCV1A2SWdcL2RLc1VxM2RZMW04LmpwZyIsInQxdVBSbFFDZDYyQUdTeFRQRFdhaUQ2eTJaM2l5bWVPc3ZzLXJ3XC9VX3BpS3AwVlVKWS5qcGciLCJwNG91ckt3MEtNOEpBcXQxNTgwSHY5N 2JvYWxWR2dSNVNINkpaQVwvbXZiNjluRTdzekkuanBnIl19LCJzcXVhcmUiOiIiLCJkYXRhIjpbIntcInBob3RvXCI6XCI1ZDg4ZmI4NjRjeFwiLFwic2l6ZXNcIjpbXSxcInNpemVzMlwiOltbXCJtYXhcIixcIjRhMWE5MGM5MjAwYWUwMDNhNzQ0YWJhZjgwNGRkMGVlODM5ZDMxYTdiNmExYjkwNzZiYTlhO Dc2XCIsXCItNzY2ODYwMjY1NTYyNTU1NTEzMVwiLDgwMCw1NjNdLFtcIm9cIixcImQyMGM1YjczYjNkMmRjMTAzZmZkY2UwY2Q5MzUyZDBkZDhjNzcxNTI2ODlhOWFlMDE2ZDI5NjFmXCIsXCItNTIyNDc2MjI0MTM5OTkyMzE3NFwiLDU2Myw1NjNdLFtcImJcIixcIjc3NGUyOTBhYjI3MDcwMzU0NmYwYzc2M 2RjNjJmYzRiMGM4YjY3Y2ZlYmQ0N2ZmYjdkOTU0Mzk4XCIsXCItNTgxODYzMzU1MzAxOTcwNjU5N1wiLDQwMCw0MDBdLFtcImFcIixcIjI1MGEyOGM0MjU3ODJiZWE1OGNkZjc3ZDBhNTcwMzM1ZGVjOGI2NWI3OWQwMTBhMTRkMzk5YWRmXCIsXCItNzA0NDg1MTQ2MzM3MTA5MzkxM1wiLDIwMCwyMDBdLFtcI mNcIixcImI0MmZmN2QyMjQ5ODRmZjFhZDUxZmI4ZjgxZDAwMGE1MmE4ZjY0MjM5NTdjZTMwYTVkODgyZTRiXCIsXCItMzIyNTM4MDkyMjYzNDE2MTMyN1wiLDIwMCwyMDBdLFtcImRcIixcImRmMTFkMjQxYjFmYzRkMWVhNGIxMjk4YzkwMzM1YzFlZGU5Y2FmODRlNDhmOWJlZWY5ODk4MTk1XCIsXCI0MzI2M jI4NTc0MTI5NzQ5OTEzXCIsMTAwLDEwMF0sW1wiZVwiLFwiNWRkZDYwOThjNjQ3MGExYWZlZDRlNGMyYTkzOWFmMTcyYTA4ZmNiYWJmNDVjYzgwMGYxZDU1NzFcIixcIi0xMTI1OTAzMzkxMjQ1MTM2Mjc4XCIsNTAsNTBdXSxcInVybHNcIjpbXSxcInVybHMyXCI6W1wiU2hxUXlTQUs0QU9uUkt1dmdFM1E3b 09kTWFlMm9ia0hhNm1vZGdcL1JWUFN3NE9tazVVLmpwZ1wiLFwiMGd4YmM3UFMzQkFfX2M0TTJUVXREZGpIY1ZKb21wcmdGdEtXSHdcL0d0NTdaV3pxZmJjLmpwZ1wiLFwiZDA0cENySndjRFZHOE1kajNHTDhTd3lMWjhfcjFIXzdmWlZEbUFcL0cyTVhxcHdQUUs4LmpwZ1wiLFwiSlFvb3hDVjRLLXBZemZkO UNsY0ROZDdJdGx0NTBCQ2hUVG1hM3dcL1p3aGdWdXVvTzU0LmpwZ1wiLFwidENfMzBpU1lUX0d0VWZ1UGdkQUFwU3FQWkNPVmZPTUtYWWd1U3dcL1ViTVJ3U01sUGRNLmpwZ1wiLFwiM3hIU1FiSDhUUjZrc1NtTWtETmNIdDZjcjRUa2o1dnUtWW1CbFFcL21mOGxndmZaQ1R3LmpwZ1wiLFwiWGQxZ21NWkhDa HItMU9UQ3FUbXZGeW9JX0xxX1JjeUFEeDFWY1FcL2FnNXR1ZFQ4WF9BLmpwZ1wiXSxcIm1hcmtlcnNfcmVzdGFydGVkXCI6dHJ1ZX0iLCIxMTgsMCw1NjMsNTYzLDU2LDU2LDQ1MCJdLCJid2FjdCI6Im93bmVyX3Bob3RvIiwic2VydmVyIjo4NTIwMzIsIm1pZCI6MzQzNTM3Njc1LCJfc2lnIjoiMWFkNmRhM DYxZGZlMTNhOWJmYzcwOGYzODVmMWIxNTkifQ";
            // var hash = getHash(responseString);
            // var hash = "7448f93f405dfd5c444f5d4aabb84942";
            var hash = responseWithPhoto["hash"].ToString();
            var savePhoto = await client.GetStringAsync($"https://api.vk.com/method/photos.saveOwnerPhoto?hash={hash}&server={server}&photos={photo}&v=5.131&access_token={access_token}");
            Console.WriteLine(savePhoto);
        }
 
        private static async Task Main()
        {
            // await UsersGet();
            // await VkAuthorize();
            await UploadPhoto();
        }
    }
