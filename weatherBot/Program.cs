using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;

namespace testDiscordBot
{
    class Program
    {
        private const string weatherKey = "WEATHER_KEY";
        private const string discordToken = "DISCORD_TOKEN";
        DiscordSocketClient client;
        DiscordSocketConfig config = new DiscordSocketConfig();

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            config.GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent;
            client = new DiscordSocketClient(config);
            client.MessageReceived += CommandsHandler;
            client.Log += Log;

            await client.LoginAsync(TokenType.Bot, discordToken);
            await client.StartAsync();

            Console.ReadLine();
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private Task CommandsHandler(SocketMessage msg)
        {
            if (!msg.Author.IsBot && msg.Content[0].ToString() == "!")
            {
                Console.WriteLine("Принято сообщение");
                string tempMessage = System.Text.RegularExpressions.Regex.Replace(msg.Content.Trim(), @"\s+", " ");
                if (tempMessage.Split().Length == 2)
                {
                    if (tempMessage.Split()[0] == "!погода")
                    {

                        string url = $"http://api.weatherapi.com/v1/current.json?key={weatherKey}&q={tempMessage.Split()[1]}";
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        string tResponse;
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            tResponse = streamReader.ReadToEnd();
                            var jsonDoc = JsonDocument.Parse(tResponse);
                            var location = jsonDoc.RootElement.GetProperty("location");
                            JsonElement name = location.GetProperty("name");
                            JsonElement localtime = location.GetProperty("localtime");
                            var current = jsonDoc.RootElement.GetProperty("current");
                            JsonElement temp_c = current.GetProperty("temp_c");
                            JsonElement feelslike_c = current.GetProperty("feelslike_c");
                            var condition = current.GetProperty("condition");
                            JsonElement code = condition.GetProperty("code");

                            string weatherStatus = "";
                            switch (code.GetInt32())
                            {
                                case 1000: weatherStatus = ":sunny:"; break;
                                case 1003: weatherStatus = ":white_sun_small_cloud:"; break;
                                case 1006: weatherStatus = ":partly_sunny:"; break;
                                case 1009: weatherStatus = ":cloud:"; break;
                                case 1030:
                                case 1135:
                                case 1147:
                                    weatherStatus = ":fog:"; break;
                                case 1063:
                                case 1066:
                                case 1069:
                                case 1072:
                                case 1087:
                                    weatherStatus = ":white_sun_rain_cloud:"; break;
                                case 1114: weatherStatus = ":cloud_snow:"; break;
                                case 1117: weatherStatus = ":dash:"; break;
                                case 1150:
                                case 1153:
                                case 1168:
                                case 1171:
                                case 1180:
                                case 1183:
                                case 1186:
                                case 1189:
                                case 1192:
                                case 1195:
                                case 1198:
                                case 1201:
                                case 1240:
                                case 1243:
                                case 1246:
                                case 1249:
                                case 1252:
                                    weatherStatus = ":cloud_rain:"; break;
                                case 1204:
                                case 1207:
                                case 1210:
                                case 1213:
                                case 1216:
                                case 1219:
                                case 1222:
                                case 1225:
                                case 1237:
                                case 1255:
                                case 1258:
                                case 1261:
                                case 1264:
                                    weatherStatus = ":cloud_snow:"; break;
                                case 1273:
                                case 1276:
                                case 1279:
                                case 1282:
                                    weatherStatus = ":thunder_cloud_rain:"; break;


                            }

                            msg.Channel.SendMessageAsync($"Населенный пункт: *{name}* {weatherStatus}" +
                            $"\nВремя в населенном пункте: *{localtime.ToString().Split()[1]}*" +
                            $"\nТекущая температура: *{temp_c}°*" +
                            $"\nОщущается как: *{feelslike_c}°*");
                            Console.WriteLine("Отправлено сообщение");
                        }
                    }
                }
                else
                {
                    msg.Channel.SendMessageAsync("Введите город (!погода *город*)");
                    Console.WriteLine("Отправлено сообщение");
                }
            }
            return Task.CompletedTask;
        }
    }
}
