using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace VkApi
{
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
            var authString =
                $"https://oauth.vk.com/authorize?client_id={clientId}&display=page&redirect_uri={redirectUri}&scope=friends&response_type=token&v=5.131&state=123456";
            authString = authString.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {authString}") { CreateNoWindow = true });

            Console.WriteLine("Введите полученный access token: ");
            var access_token = Console.ReadLine();
            var json = await client.GetStringAsync(
                $"https://api.vk.com/method/friends.getOnline?user_id={userId}&v=5.131&access_token={access_token}");

            var (startIndex, endIndex) = (json.IndexOf("["), json.IndexOf("]"));

            var friendsList = json.Substring(startIndex + 1, endIndex - startIndex - 1).Split(',')
                .Select(prop => int.Parse(prop)).ToList();


            foreach (var id in friendsList)
            {
                var userResponse = await client.GetStringAsync(
                    $"https://api.vk.com/method/users.get?user_id={id}&v=5.131&fields=bdate&access_token={access_token}");
                var userAccount = JsonConvert.DeserializeObject<Response>(userResponse);
                try
                {
                    Console.WriteLine(userAccount.response[0].first_name + " " + userAccount.response[0].last_name);
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Ошибка чтения данных");
                }

                Thread.Sleep(500);
            }
            Console.WriteLine("Нажмите <Enter>, чтобы закрыть консоль");
            Console.ReadLine(); // задержка консоли. Актуально только для Windows
        }


        private static async Task UploadPhoto()
        {
            Console.WriteLine("Введите путь к фото: ");
            var photo_path = Console.ReadLine();
            var startIndex = Math.Max(photo_path.LastIndexOf("/"), photo_path.LastIndexOf("\\"));
            var full_file_name = photo_path.Substring(startIndex + 1);
            var form = new MultipartFormDataContent
                { { new ByteArrayContent(File.ReadAllBytes(photo_path)), "photo", full_file_name } };
            var client = new HttpClient();
            const int clientId = 7499511;
            const string redirectUri = "https://oauth.vk.com/blank.html";
            Console.WriteLine("Введите ID пользователя или группы для изменения фото (целое число): ");
            var userId = int.Parse(Console.ReadLine());
            var authString =
                $"https://oauth.vk.com/authorize?client_id={clientId}&display=page&redirect_uri={redirectUri}&scope=friends&response_type=token&v=5.131&state=123456";
            authString = authString.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {authString}") { CreateNoWindow = true });
            Console.WriteLine("Введите полученный access token: ");
            string access_token = Console.ReadLine();

            //to get link
            var json = await client.GetStringAsync(
                $"https://api.vk.com/method/photos.getOwnerPhotoUploadServer?owner_id={userId}&v=5.131&access_token={access_token}");
            JObject responseWithUrl = JObject.Parse(json);
            var url = responseWithUrl["response"]["upload_url"].ToString();
            var response = await client.PostAsync(url, form);

            var responseString = await response.Content.ReadAsStringAsync();
            JObject responseWithPhoto = JObject.Parse(responseString);

            var server = responseWithPhoto["server"].ToString();
            var photo = responseWithPhoto["photo"].ToString();
            var hash = responseWithPhoto["hash"].ToString();
            var savePhoto = await client.GetStringAsync(
                $"https://api.vk.com/method/photos.saveOwnerPhoto?hash={hash}&server={server}&photos={photo}&v=5.131&access_token={access_token}");

            Console.WriteLine("Нажмите <Enter>, чтобы закрыть консоль");
            Console.ReadLine(); // задержка консоли. Актуально только для Windows
        }

        private static async Task Main()
        {
            await UploadPhoto();
        }
    }
}
