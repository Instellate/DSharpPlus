using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DSharpPlus.EventArgs;
using DSharpPlus.CH.Message.Internals;

namespace DSharpPlus.CH.Internals
{
    internal class CommandController
    {
        private MessageCommandFactory _messageFactory;
        public MessageCommandFactory MessageCommandFactory { get => _messageFactory; private set => _messageFactory = value; }
        public CHConfiguration Configuration { get; set; }
        public ServiceProvider Services { get; set; }

        public CommandController(CHConfiguration configuration, DiscordClient client)
        {
            Configuration = configuration;
            Services = Configuration.Services?.BuildServiceProvider()
                    ?? new ServiceCollection().BuildServiceProvider();
            _messageFactory = new MessageCommandFactory
            {
                _services = Services,
            };

            CommandModuleRegister.RegisterMessageCommands(_messageFactory, Configuration.Assembly);
            client.MessageCreated += HandleMessageCreation;
        }

        public async Task HandleMessageCreation(DiscordClient client, MessageCreateEventArgs msg)
        {
            if (msg.Author.IsBot) return;
            if (Configuration.Prefix is null) return;
            if (!msg.Message.Content.StartsWith(Configuration.Prefix)) return;

            var content = msg.Message.Content.Remove(0, Configuration.Prefix.Count()).Split(' ');
            if (content.Count() == 0) return;

            var command = content[0];
            var args = content.Skip(1).ToArray(); // Command argument parsing needed here.
            await _messageFactory.ExecuteCommand(command, msg.Message, client, args);
        }
    }
}
