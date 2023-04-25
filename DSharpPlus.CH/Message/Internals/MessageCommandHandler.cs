using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message.Internals
{
    internal class MessageCommandHandler
    {
        private MessageCommandModule _module = null!; // Will be set by the factory.
        private DiscordMessage? _newMessage;

        internal async Task TurnResultIntoAction(IMessageCommandModuleResult result)
        {
            // [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            void SetContentAndEmbeds(IMessageCommandModuleResult result, DiscordMessageBuilder messageBuilder)
            {
                if (result.Content is not null) messageBuilder.WithContent(result.Content);
                if (result.Embeds is not null) messageBuilder.AddEmbeds(result.Embeds);
            }

            var msgBuilder = new DiscordMessageBuilder();
            switch (result.Type)
            {
                case MessageCommandModuleResultType.Empty: return;
                case MessageCommandModuleResultType.Reply:
                    SetContentAndEmbeds(result, msgBuilder);
                    msgBuilder.WithReply(_module.Message.Id, false);

                    _newMessage = await _module.Message.Channel.SendMessageAsync(msgBuilder);
                    break;
                case MessageCommandModuleResultType.NoMentionReply:
                    SetContentAndEmbeds(result, msgBuilder);
                    msgBuilder.WithReply(_module.Message.Id, true);

                    _newMessage = await _module.Message.Channel.SendMessageAsync(msgBuilder);
                    break;
                case MessageCommandModuleResultType.Send:
                    SetContentAndEmbeds(result, msgBuilder);

                    _newMessage = await _module.Message.Channel.SendMessageAsync(msgBuilder);
                    break;
                case MessageCommandModuleResultType.FollowUp:
                    SetContentAndEmbeds(result, msgBuilder);
                    if (_newMessage is not null) msgBuilder.WithReply(_newMessage.Id);

                    await _module.Message.Channel.SendMessageAsync(msgBuilder);
                    break;
                case MessageCommandModuleResultType.Edit:
                    SetContentAndEmbeds(result, msgBuilder);
                    if (_newMessage is not null) await _newMessage.ModifyAsync(msgBuilder);
                    break;
            }
        }

        internal async Task BuildModuleAndExecuteCommand(MessageCommandMethodData data, ServiceProvider services, DiscordMessage message, DiscordClient client, Dictionary<string, object> options, Queue<string> arguments)
        {
            var moduleData = data.Module;
            using var scoped = services.CreateScope();
            object?[]? constructorParams = null;

            var constructors = moduleData.Type.GetConstructors();
            if (constructors is not null && constructors.Count() != 0)
            {
                var constructor = constructors[0];
                var constructorParameters = constructor.GetParameters();
                constructorParams = new object[constructorParameters.Count()];
                for (int i = 0; i < constructorParameters.Count(); i++)
                {
                    var type = constructorParameters[i].ParameterType;
                    constructorParams[i] = scoped.ServiceProvider.GetService(type);
                }
                _module = (Activator.CreateInstance(moduleData.Type, constructorParams) as MessageCommandModule)!;
            }
            else _module = (Activator.CreateInstance(moduleData.Type, null) as MessageCommandModule)!;

            _module.Message = message;
            _module._handler = this;
            _module.Client = client;

            object?[]? parameters = null;
            if (data.Parameters.Count() != 0)
            {
                parameters = new object?[data.Parameters.Count()];
                for (int i = 0; i < data.Parameters.Count; i++)
                {
                    var parameter = data.Parameters[i];

                    if (parameter.IsArgument)
                    {
                        try
                        {
                            parameters[i] = await ParseParameter(parameter, arguments.Dequeue(), client);
                        }
                        catch (Exceptions.ConvertionFailedException e)
                        {
                            var error = new InvalidMessageConvertionError
                            {
                                Value = e.Value,
                                Type = e.Type,
                                Name = parameter.Name,
                                IsArgument = parameter.IsArgument
                            };
                            await MessageErrorHandler.HandleInvalidOption(error, _module.Message);
                            return;
                        }
                    }
                    else
                    {
                        if (options.TryGetValue(parameter.Name, out var value))
                            try
                            {
                                parameters[i] = await ParseParameter(parameter, value, client);
                            }
                            catch (Exceptions.ConvertionFailedException e)
                            {
                                var error = new InvalidMessageConvertionError
                                {
                                    Value = e.Value,
                                    Type = e.Type,
                                    Name = parameter.Name,
                                    IsArgument = parameter.IsArgument
                                };
                                await MessageErrorHandler.HandleInvalidOption(error, _module.Message);
                                return;
                            }
                        else if (parameter.ShorthandOptionName is not null && options.TryGetValue(parameter.ShorthandOptionName, out var val))
                            try
                            {
                                parameters[i] = await ParseParameter(parameter, val, client);
                            }
                            catch (Exceptions.ConvertionFailedException e)
                            {
                                var error = new InvalidMessageConvertionError
                                {
                                    Value = e.Value,
                                    Type = e.Type,
                                    Name = parameter.Name,
                                    IsArgument = parameter.IsArgument
                                };
                                await MessageErrorHandler.HandleInvalidOption(error, _module.Message);
                                return;
                            }
                        else if (parameter.Type == MessageCommandParameterDataType.Bool)
                            parameters[i] = false;
                        else if (parameter.CanBeNull)
                            parameters[i] = null;
                    }
                }
            }

            if (data.IsAsync)
            {
                var task = (Task<IMessageCommandModuleResult>)data.Method.Invoke(_module, parameters)!;
                var result = await task;
                await TurnResultIntoAction(result);
            }
            else
            {
                var result = (IMessageCommandModuleResult)data.Method.Invoke(_module, parameters)!;
                await TurnResultIntoAction(result);
            }
        }

        private async Task<object> ParseParameter(MessageCommandParameterData data, object value, DiscordClient client)
        {
            object obj = new object();

            if (data.Type == MessageCommandParameterDataType.Bool && value.GetType() != typeof(bool))
            {
                throw new Exceptions.ConvertionFailedException(((bool)value).ToString(), InvalidMessageConvertionType.BoolShouldNotHaveValue);
            }
            else if (data.Type != MessageCommandParameterDataType.Bool && value.GetType() == typeof(bool))
            {
                throw new Exceptions.ConvertionFailedException(((bool)value).ToString(), InvalidMessageConvertionType.NoValueProvided);
            }

            switch (data.Type)
            {
                case MessageCommandParameterDataType.String:
                    obj = value;
                    break;
                case MessageCommandParameterDataType.Int:
                    {
                        if (int.TryParse((string)value, out var result))
                        {
                            obj = result;
                        }
                        else
                            throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.NotAValidInteger);
                        break;
                    }
                case MessageCommandParameterDataType.Double:
                    {
                        if (double.TryParse((string)value, out var result))
                        {
                            obj = result;
                        }
                        break;
                    }
                case MessageCommandParameterDataType.User:
                    {
                        string str = (string)value;

                        if (str.StartsWith("<@"))
                        {
                            if (str.Count() >= 3 && str[2] == '!')
                                str = str.Remove(0, 3);
                            else
                                str = str.Remove(0, 2);

                            str = str.Remove(str.Count() - 1, 1);
                            if (ulong.TryParse(str, out var result))
                            {
                                try
                                {
                                    obj = await client.GetUserAsync(ulong.Parse(str));
                                }
                                catch (DSharpPlus.Exceptions.NotFoundException)
                                {
                                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.UserDoesNotExist);
                                }
                                catch (DSharpPlus.Exceptions.ServerErrorException)
                                {
                                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.UserDoesNotExist);
                                }
                            }
                            else
                                throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.NotAValidUser);
                        }
                        else
                        {
                            if (ulong.TryParse((string)value, out var result))
                            {
                                obj = await client.GetUserAsync(result);
                            }
                            else
                                throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.NotAValidUser);
                        }
                        break;
                    }
                case MessageCommandParameterDataType.Member:
                    {
                        if (_module.Message.Channel.GuildId is null)
                        {
                            break;
                        }

                        string str = (string)value;
                        if (str.StartsWith("<@"))
                        {
                            if (str.Count() >= 3 && str[2] == '!')
                                str = str.Remove(0, 3);
                            else
                                str = str.Remove(0, 2);

                            str = str.Remove(str.Count() - 1, 1);
                            if (ulong.TryParse(str, out var result))
                            {
                                try
                                {
                                    obj = await _module.Message.Channel.Guild.GetMemberAsync(ulong.Parse(str));
                                }
                                catch (DSharpPlus.Exceptions.NotFoundException)
                                {
                                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.MemberDoesNotExist);
                                }
                                catch (DSharpPlus.Exceptions.ServerErrorException)
                                {
                                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.MemberDoesNotExist);
                                }
                            }
                            else
                                throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.NotAValidMember);
                        }
                        else
                        {
                            if (ulong.TryParse((string)value, out var result))
                            {
                                try
                                {
                                    obj = await _module.Message.Channel.Guild.GetMemberAsync(result);
                                }
                                catch (DSharpPlus.Exceptions.NotFoundException)
                                {
                                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.MemberDoesNotExist);
                                }
                                catch (DSharpPlus.Exceptions.ServerErrorException)
                                {
                                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.MemberDoesNotExist);
                                }
                            }
                            else
                                throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.NotAValidMember);
                        }


                        break;
                    }
                case MessageCommandParameterDataType.Channel:
                    {
                        if (_module.Message.Channel.GuildId is null)
                        {
                            break;
                        }

                        string str = (string)value;

                        if (str.StartsWith("<#"))
                        {
                            if (str.Count() >= 3 && str[2] == '!')
                                str = str.Remove(0, 3);
                            else
                                str = str.Remove(0, 2);
                            str = str.Remove(str.Count() - 1, 1);

                            if (ulong.TryParse(str, out var result))
                            {
                                try
                                {
                                    obj = _module.Message.Channel.Guild.GetChannel(result);
                                }
                                catch (DSharpPlus.Exceptions.ServerErrorException)
                                {
                                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.ChannelDoesNotExist);
                                }
                                catch (DSharpPlus.Exceptions.NotFoundException)
                                {
                                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.ChannelDoesNotExist);
                                }
                            }
                            else
                                throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.NotAValidChannel);
                        }
                        break;
                    }
                case MessageCommandParameterDataType.Bool:
                    obj = value;
                    break;

            }

            return obj;
        }

    }
}
