using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LightenceClient
{
    static class HttpClientManager
    {
        // task for endpoint: /identity/login
        public static async Task<HttpResponseMessage> LoginUserAsync(string email, string password)
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            var Data = new
            {
                username = email,
                password = password
            };
            HttpResponseMessage response = await client.PostAsync("/identity/login",
                new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json"));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Login success";
                Loggers.Comm_logger.Info(msg);
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Login: StatusCode " + code.ToString();
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Login fail";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Login: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            return response;
        }

        // task for endpoint: /identity/loginvision
        public static async Task<HttpResponseMessage> LoginVisionUserAsync(string email, byte[] image)
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            var Data = new
            {
                username = email,
                data = image
            };
            HttpResponseMessage response = await client.PostAsync("/identity/loginvision",
                new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json"));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Biometric login success";
                Loggers.Comm_logger.Info(msg);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync())["content"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Vision Login: StatusCode " + code.ToString() + "; Content " + content;
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Biometric login fail";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Vision Login: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            return response;
        }

        // task for endpoint: /identity/addvision
        public static async Task<HttpResponseMessage> AddVisionProfileAsync(string email, byte[] image)
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            var Data = new
            {
                username = email,
                data = image
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWToken.Token);
            HttpResponseMessage response = await client.PostAsync("/identity/addvision",
                new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json"));

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Biometric registration success";
                Loggers.Comm_logger.Info(msg);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync())["content"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Add Vision: StatusCode " + code.ToString() + "; Content " + content;
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Biometric registration fail";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Add Vision: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            return response;
        }

        public static async Task<HttpResponseMessage> RemoveVisionProfileAsync()
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWToken.Token);
            HttpResponseMessage response = await client.DeleteAsync("/identity/deletevision");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Biometric remove success";
                Loggers.Comm_logger.Info(msg);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync())["content"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Delete Vision: StatusCode " + code.ToString() + "; Content " + content;
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Biometric registration fail";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Delete Vision: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            return response;
        }

        // task for endpoint: /identity/logout
        public static async Task<HttpResponseMessage> LogoutUserAsync()
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWToken.Token);
            HttpResponseMessage response = await client.GetAsync("/identity/logout");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Logout success";
                Loggers.Comm_logger.Info(msg);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync())["content"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Logout: StatusCode " + code.ToString() + "; Content" + content;
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Logout fail";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Logout: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            return response;
        }

        // task for endpoint: /identity/register
        public static async Task<HttpResponseMessage> RegisterUserAsync(string username, string password, string firstname, string lastname)
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            var Data = new
            {
                username = username,
                password = password,
                firstname = firstname,
                lastname = lastname
            };
            HttpResponseMessage response = await client.PostAsync("/identity/register",
                new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json"));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Account created";
                Loggers.Comm_logger.Info(msg);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync())["content"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Register User: StatusCode " + code.ToString() + "; Content " + content;
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Account not created";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager Register User: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            return response;
        }

        // task for endpoint: /identity/update
        public static async Task<HttpResponseMessage> UpdateUserAsync(string firstname, string lastname)
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWToken.Token);
            var Data = new
            {
                FirstName = firstname,
                LastName = lastname
            };
            HttpResponseMessage response = await client.PutAsync("/identity/update",
                new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json"));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Update account success";
                Loggers.Comm_logger.Info(msg);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync())["content"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Update User: StatusCode " + code.ToString() + "; Content " + content;
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Update account fail";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager Update User: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            //response.EnsureSuccessStatusCode();
            return response;
        }

        // task for endpoint: /identity/delete
        public static async Task<HttpResponseMessage> DeleteUserAsync()
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWToken.Token);
            HttpResponseMessage response = await client.DeleteAsync("/identity/delete");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Delete account success";
                Loggers.Comm_logger.Info(msg);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync())["content"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Delete: StatusCode " + code.ToString() + "; Content " + content;
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Delete account fail";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Delete: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            //response.EnsureSuccessStatusCode();
            return response;
        }

        // task for endpoint: /identity/changepass
        public static async Task<HttpResponseMessage> ChangepassUserAsync(string oldpassword, string newpassword)
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWToken.Token);
            var Data = new
            {
                OldPassword = oldpassword,
                NewPassword = newpassword
            };
            HttpResponseMessage response = await client.PutAsync("/identity/changepass",
                new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json"));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Change password success";
                Loggers.Comm_logger.Info(msg);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync())["content"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Change Password: StatusCode " + code.ToString() + "; Content " + content;
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Change password fail";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Change Password: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            //response.EnsureSuccessStatusCode();
            return response;
        }

        public static async Task<HttpResponseMessage> ResetpassUserAsync(string userName)
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            HttpResponseMessage response = await client.PostAsync("/identity/resetpassword",
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Reset password success";
                Loggers.Comm_logger.Info(msg);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync())["content"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Reset Password: StatusCode " + code.ToString() + "; Content " + content;
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Reset password fail";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Reset Password: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            //response.EnsureSuccessStatusCode();
            return response;
        }

        // task for endpoint: /identity/premium
        public static async Task<HttpResponseMessage> PremiumUserAsync(string appkey)
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri(Constants._serverAddress) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWToken.Token);
            var Data = new
            {
                Appkey = appkey
            };
            HttpResponseMessage response = await client.PostAsync("/identity/premium",
                new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json"));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string msg = "Update account to premium success";
                Loggers.Comm_logger.Info(msg);
                var content = JObject.Parse(await response.Content.ReadAsStringAsync())["content"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Premium: StatusCode " + code.ToString() + "; Content " + content;
                Loggers.Comm_logger.Debug(msg);
            }
            else
            {
                string msg = "Update account to premium fail";
                Loggers.Comm_logger.Error(msg);
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"].ToString();
                int code = (int)response.StatusCode;
                msg = "HttpClientManager - Premium: StatusCode " + code.ToString() + "; Error " + error;
                Loggers.Comm_logger.Debug(msg);
            }
            //response.EnsureSuccessStatusCode();
            return response;
        }
    }
}