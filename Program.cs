//"6737345303:AAGlFQ3qRwLBSCG_uBsgBpNYqwfGiSZnAoM"
using System;
using DotNetTools.SharpGrabber;
using DotNetTools.SharpGrabber.Exceptions;
using DotNetTools.SharpGrabber.Auth;
using DotNetTools.SharpGrabber.Grabbed;
using DotNetTools.SharpGrabber.Converter;
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
            _botClient = new TelegramBotClient("6737345303:AAGlFQ3qRwLBSCG_uBsgBpNYqwfGiSZnAoM"); // Replace with your actual API token

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
                        break; // Case start

                    // Inside the "Create video" case
                    case "Create video":
                        BotState.IsWaitingForVideoLink = true;

                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat.Id,
                            text: "Please provide a link to the YouTube video:"
                        );

                        // Handler for receiving video link
                        _botClient.OnMessage += async (sender, args) =>
                        {
                            if (BotState.IsWaitingForVideoLink && args.Message.Text != null && args.Message.Chat.Id == e.Message.Chat.Id)
                            {
                                var youtubeLink = args.Message.Text;

                                var uri = new Uri(youtubeLink); 

                                var grabber = GrabberFactory.GetGrabber(uri);
                                var result = await grabber.GrabAsync(uri);

                                if (result.IsSuccessful && result.Metadata != null && result.Metadata.Content != null)
                                {
                                    var video = result.Metadata.Content;

                                    // Extract the best quality video URL from the parsed video metadata
                                    var bestQualityVideo = video.GetBestStream();

                                    if (bestQualityVideo != null)
                                    {
                                        BotState.IsWaitingForVideoLink = false; // Stop waiting for the link

                                        using (var httpClient = new HttpClient())
                                        {
                                            var response = await httpClient.GetAsync(bestQualityVideo.Url);

                                            if (response.IsSuccessStatusCode)
                                            {
                                                var videoBytes = await response.Content.ReadAsByteArrayAsync();

                                                // Now you have the video file in videoBytes variable
                                                // You can further process or send this video file as needed

                                                // Inform the user that the video file is being processed or perform other actions
                                                await _botClient.SendTextMessageAsync(
                                                    chatId: args.Message.Chat.Id,
                                                    text: "Video file received. Processing..."
                                                );
                                            }
                                            else
                                            {
                                                await _botClient.SendTextMessageAsync(
                                                    chatId: args.Message.Chat.Id,
                                                    text: "Failed to fetch the video file. Please try again later."
                                                );
                                            }
                                        }
                                    }
                                    else if (args.Message.Text.Equals("/stop", StringComparison.OrdinalIgnoreCase))
                                    {
                                        BotState.IsWaitingForVideoLink = false; // Stop waiting for the link

                                        await _botClient.SendTextMessageAsync(
                                            chatId: args.Message.Chat.Id,
                                            text: "Operation canceled."
                                        );
                                    }
                                    else
                                    {
                                        // Inform the user about the issue in finding the video stream
                                        await _botClient.SendTextMessageAsync(
                                            chatId: args.Message.Chat.Id,
                                            text: "Failed to retrieve the video stream. Please try again later or type /stop to cancel."
                                        );
                                    }
                                }
                                else if (args.Message.Text.Equals("/stop", StringComparison.OrdinalIgnoreCase))
                                {
                                    BotState.IsWaitingForVideoLink = false; // Stop waiting for the link

                                    await _botClient.SendTextMessageAsync(
                                        chatId: args.Message.Chat.Id,
                                        text: "Operation canceled."
                                    );
                                }
                                else
                                {
                                    // Inform the user to provide a valid YouTube link
                                    await _botClient.SendTextMessageAsync(
                                        chatId: args.Message.Chat.Id,
                                        text: "Invalid YouTube link. Please provide a correct link to the YouTube video or type /stop to cancel."
                                    );
                                }
                            }
                        };
                        break; // Case Create Video

                    case "Balance":
                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat.Id,
                            text: "Your current balance is..."
                        );
                        break; //Case Balance

                    case "Stop Bot":
                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat.Id,
                            text: "Your current balance is..."
                        );
                        break; //Case Stop

                    default:
                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat.Id,
                            text: "Invalid command. Please use one of the provided options."
                        );
                        break;
                }
            }
        }
    }
}
