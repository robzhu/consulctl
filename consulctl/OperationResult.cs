
namespace Consulctl
{
    public enum OperationResultCode : int
    {
        Success = 0,
        GenericError,
        HelpRequested,
        NoArguments,
        ArgumentsParsingError,
        HostNotReachable,
        ServiceDefinitionFileNotFound,
        ServiceDefinitionFileBadFormat,
        RegisterServiceFailure,
        UnregisterServiceFailure,
        MainOptionMissing,
        MutlipleMainOptions,
        SubOptionMissing,
        MutlipleSubOptions,
        InvalidHostUri, 
        InvalidKey,
        ValueCannotBeNullOrEmpty,
        CreateKeyFailure,
        DeleteKeyFailure,
        DeleteNodeFailure,
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool ShowHelp { get; set; }
        public string HelpText { get; set; }
        public OperationResultCode Code { get; set; }

        public bool ShowValue { get; set; }
        public string Value { get; set; }
    }

    public static class OperationResults
    {
        public static OperationResult CreateResult( this ConsulCommandLineTool commandTool, OperationResultCode code )
        {
            var result = new OperationResult()
            {
                Success = false,
                Code = code,
            };
            switch( code )
            {
                case OperationResultCode.Success:
                    result.Message = "Operation completed successfully.";
                    result.Success = true;
                    break;

                case OperationResultCode.GenericError:
                    result.Message = "An unexpected error has occured.";
                    break;

                case OperationResultCode.HelpRequested:
                    result.ShowHelp = true;
                    result.HelpText = commandTool.GetUsage();
                    break;

                case OperationResultCode.NoArguments:
                    result.Message = "No arguments were provided.";
                    result.ShowHelp = true;
                    result.HelpText = commandTool.GetUsage();
                    break;

                case OperationResultCode.ArgumentsParsingError:
                    result.Message = "Encountered an error while parsing the arguments.";
                    result.ShowHelp = true;
                    result.HelpText = commandTool.GetUsage();
                    break;

                case OperationResultCode.HostNotReachable:
                    result.Message = string.Format( "The specified host could not be reached: {0}", commandTool.Client.Host.ToString() );
                    break;

                case OperationResultCode.ServiceDefinitionFileNotFound:
                    result.Message = string.Format( "The specified service definition file could not be found: {0}", commandTool.Options.Service );
                    break;

                case OperationResultCode.ServiceDefinitionFileBadFormat:
                    result.Message = string.Format( "The specified service definition file is not valid: {0}", commandTool.Options.Service );
                    break;

                case OperationResultCode.RegisterServiceFailure:
                    result.Message = string.Format( "Failed to register the service." );
                    break;

                case OperationResultCode.UnregisterServiceFailure:
                    result.Message = string.Format( "Failed to unregister the service, likely because a service with that id does not exist." );
                    break;

                case OperationResultCode.MainOptionMissing:
                    result.Message = "Need to specify at least one main option: \n" +
                                     "-n --node:    node \n" +
                                     "-s --svc:     service definition \n" +
                                     "-k --kv:      key/value";
                    break;

                case OperationResultCode.MutlipleMainOptions:
                    result.Message = "Detected multiple main options. Specify only one main option: \n" +
                                     "-n --node:    node \n" +
                                     "-s --svc:     service definition \n" +
                                     "-k --kv:      key/value";
                    break;

                case OperationResultCode.SubOptionMissing:
                    result.Message = "Need to specify at least one action option: \n" +
                                     "-c --create:  create the key/value or service\n" +
                                     "-r --read:    read a key/value or service\n" + 
                                     "-d --delete:  delete a key/value or service\n";
                    break;

                case OperationResultCode.MutlipleSubOptions:
                    result.Message = "Detected multiple action options. Specify only one action option: \n" +
                                     "-c --create:  create the key/value or service\n" +
                                     "-r --read:    read a key/value or service\n" +
                                     "-d --delete:  delete a key/value or service\n";
                    break;

                case OperationResultCode.InvalidHostUri:
                    result.Message = string.Format( "The host uri is not valid: {0}", commandTool.Options.GetHostString() );
                    break;

                case OperationResultCode.InvalidKey:
                    result.Message = string.Format( "The specified key is not valid: {0}", commandTool.Options.Key );
                    break;

                case OperationResultCode.DeleteKeyFailure:
                    result.Message = string.Format( "Failed to delete the key: {0}", commandTool.Options.Key );
                    break;

                case OperationResultCode.CreateKeyFailure:
                    result.Message = string.Format( "Failed to create the key: {0}", commandTool.Options.Key );
                    break;

                case OperationResultCode.ValueCannotBeNullOrEmpty:
                    result.Message = string.Format( "Cannot create a key without a value" );
                    break;

                case OperationResultCode.DeleteNodeFailure:
                    result.Message = string.Format( "Cannot delete the specified node, likely because it does not exist." );
                    break;
            }

            return result;
        }
    }
}
