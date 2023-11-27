//"6737345303:AAGlFQ3qRwLBSCG_uBsgBpNYqwfGiSZnAoM"
using System;
using Telegram.Bot;
using Telegram.Bot.Args;

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

            _botClient.OnMessage += async (sender, e) =>
            {
                if (e.Message.Text != null)
                {
                    Console.WriteLine($"Received a message from {e.Message.Chat.Id}: {e.Message.Text}");

                    // Respond to the received message
                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        text: "Hello! I'm a simple bot."
                    );
                }
            };

            _botClient.StartReceiving();

            Console.WriteLine("Bot started. Press any key to exit.");
            Console.ReadKey();

        }
    }
}


