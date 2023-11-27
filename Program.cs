//"6737345303:AAGlFQ3qRwLBSCG_uBsgBpNYqwfGiSZnAoM"
using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace YourNamespace
{
    class Program
    {
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

                if (e.Message.Text.Equals("/start"))
                {
                    // Welcome message and command options
                    var replyMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            new KeyboardButton("/create video"),
                            new KeyboardButton("/balance"),
                            new KeyboardButton("/stop")
                        }
                    });

                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        text: "Welcome! Please choose an option:",
                        replyMarkup: replyMarkup
                    );
                }
                else if (e.Message.Text.Equals("/create video"))
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        text: "Creating a video..."
                    );
                    // Implement functionality for creating a video here
                }
                else if (e.Message.Text.Equals("/balance"))
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        text: "Your current balance is..."
                    );
                    // Implement functionality for checking balance here
                }
                else if (e.Message.Text.Equals("/stop"))
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        text: "Stopping the bot. Goodbye!"
                    );
                    // Implement functionality to stop the bot or take necessary actions before stopping
                }
                else
                {
                    // Default message for unrecognized commands
                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        text: "Invalid command. Please use one of the provided options."
                    );
                }
            }
        }
    }
}
