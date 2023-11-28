//"6737345303:AAGlFQ3qRwLBSCG_uBsgBpNYqwfGiSZnAoM"
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetTools.SharpGrabber;
using DotNetTools.SharpGrabber.Exceptions;
using DotNetTools.SharpGrabber.Grabbed;
using DotNetTools.SharpGrabber.YouTube;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace YourNamespace
{
    class Program
    {
        public static class BotState
        {
            public static bool IsWaitingForVideoLink = false;
        }

        private static TelegramBotClient _botClient;

        static void Main(string[] args)
        {
            _botClient = new TelegramBotClient("6737345303:AAGlFQ3qRwLBSCG_uBsgBpNYqwfGiSZnAoM");

            var me = _botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello! I am user {me.Id} and my name is {me.FirstName}");

            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving();

            Console.WriteLine("Bot started. Press any key to exit.");
            Console.ReadKey();

            _botClient.StopReceiving();
        }

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Console.WriteLine($"Received a message from {e.Message.Chat.Id}: {e.Message.Text}");

                switch (e.Message.Text)
                {
                    case "/start":
                        var replyMarkup = new ReplyKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                new KeyboardButton("Create video"),
                                new KeyboardButton("Balance"),
                                new KeyboardButton("Stop Bot")
                            }
                        });

                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat.Id,
                            text: "Welcome! Please choose an option:",
                            replyMarkup: replyMarkup
                        );
                        break;

                    case "Create video":
                        BotState.IsWaitingForVideoLink = true;

                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat.Id,
                            text: "Please provide a link to the YouTube video:"
                        );
                        break;

                    case "Balance":
                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat.Id,
                            text: "Your current balance is..."
                        );
                        break;

                    case "Stop Bot":
                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat.Id,
                            text: "Your current balance is..."
                        );
                        break;

                    default:
                        if (BotState.IsWaitingForVideoLink)
                        {
                            await ProcessYouTubeLink(e.Message.Text, e.Message.Chat.Id);
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat.Id,
                                text: "Invalid command. Please use one of the provided options."
                            );
                        }
                        break;
                }
            }
        }

        private static async Task ProcessYouTubeLink(string youtubeLink, long chatId)
        {
            try
            {
                var grabber = GrabberBuilder.New()
                    .UseDefaultServices()
                    .AddYouTube()
                    .Build(); ;//.CreateBuilder().Build();

                var result = await grabber.GrabAsync(new Uri(youtubeLink));

                if (result != null)
                {
                    var video =  result.Resources<GrabbedMedia>();

                    if (video != null)
                    {
                        var bestQualityStream = video.GetHighestQualityVideo();

                        if (bestQualityStream != null)
                        {
                            using (var httpClient = new HttpClient())
                            {
                                var response = await httpClient.GetAsync(bestQualityStream.OriginalUri.ToString());

                                if (response.IsSuccessStatusCode)
                                {
                                    var videoBytes = await response.Content.ReadAsByteArrayAsync();

                                    // Now you have the video file in videoBytes variable
                                    // You can further process or send this video file as needed

                                    // Inform the user that the video file is being processed or perform other actions
                                    await _botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "Video file received. Processing..."
                                    );

                                    // Example: Sending the video file to the user (you may need to adapt this based on your requirements)
                                    using (var videoStream = new MemoryStream(videoBytes))
                                    {
                                        await _botClient.SendVideoAsync(
                                            chatId: chatId,
                                            video: new Telegram.Bot.Types.InputFiles.InputOnlineFile(videoStream, "YourVideoFileName")
                                        // Additional parameters can be added here as needed
                                        );
                                    }
                                }
                                else
                                {
                                    await _botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "Failed to fetch the video file. Please try again later."
                                    );
                                }
                            }
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Failed to retrieve the video stream. Please try again later."
                            );
                        }
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Failed to retrieve YouTube video metadata."
                        );
                    }
                }
                else
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Invalid YouTube link. Please provide a correct link to the YouTube video."
                    );
                }
            }
            catch (GrabException ex)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Failed to process the YouTube link: {ex.Message}"
                );
            }
        }
    }
}
