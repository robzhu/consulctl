using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Consul;

namespace Consulctl
{
    public class ConsulCommandLineTool
    {
        private IWebClient _client = null;
        public IWebClient Client 
        {
            get
            {
                if( _client == null )
                {
                    _client = new WebClient( Options.GetHostString() );
                }
                return _client;
            }
            set{ _client = value; }
        }

        public Options Options { get; private set; }

        public OperationResult Process( string[] args )
        {
            try
            {
                return ProcessAsync( args ).Result;
            }
            catch( AggregateException ae )
            {
                if( ( ae.InnerExceptions.Count == 1 ) )
                {
                    OperationException oex = ae.InnerException as OperationException;
                    if( oex != null )
                    {
                        return oex.Result;
                    }
                }
                throw;
            }
        }

        public string GetUsage()
        {
            return new Options().GetUsage();
        }

        private OperationException OperationException( OperationResultCode code )
        {
            return new OperationException( this.CreateResult( code ) );
        }

        private ServiceDefinition ParseServiceDefinition()
        {
            if( !File.Exists( Options.ServiceArgument ) )
                throw OperationException( OperationResultCode.ServiceDefinitionFileNotFound );

            try
            {
                var sr = new StreamReader( Options.ServiceArgument );
                var serviceJson = sr.ReadToEnd();
                return ServiceDefinition.CreateFromJson( serviceJson );
            }
            catch
            {
                throw OperationException( OperationResultCode.ServiceDefinitionFileBadFormat );
            }
        }

        private string ParseValue()
        {
            if( File.Exists( Options.Value ) )
            {
                var sr = new StreamReader( Options.Value );
                return sr.ReadToEnd();
            }
            else
            {
                return Options.Value;
            }
        }

        private void ValidateUri()
        {
            try
            {
                var uri = new Uri( Options.GetHostString() );
            }
            catch
            {
                throw OperationException( OperationResultCode.InvalidHostUri );
            }
        }

        private void ValidateKey()
        {
            try
            {
                var uri = new Uri( Options.GetHostString() + "/v1/kv/" + Options.Key );
            }
            catch
            {
                throw OperationException( OperationResultCode.InvalidKey );
            }
        }

        private void ValidateValue()
        {
            if( string.IsNullOrEmpty( Options.Value ) )
            {
                throw OperationException( OperationResultCode.ValueCannotBeNullOrEmpty );
            }
        }

        private async Task ValidateHostReachableAsync()
        {
            bool hostReachable = await Client.IsHostReachableAsync();
            if( !hostReachable )
            {
                throw OperationException( OperationResultCode.HostNotReachable );
            }
        }

        public async Task<OperationResult> ProcessAsync( string[] args )
        {
            if( ( args == null ) || ( args.Length == 0 ) ) return this.CreateResult( OperationResultCode.NoArguments );
            if( args.Any( arg => arg == "--help" ) ) return this.CreateResult( OperationResultCode.HelpRequested );

            var parser = new Parser( new ParserSettings()
            {
                CaseSensitive = false,
                HelpWriter = null,
                IgnoreUnknownArguments = false,
            } );

            Options = new Options();
            parser.ParseArguments( args, Options );

            if( Options.IsMissingMainOption() ) return this.CreateResult( OperationResultCode.MainOptionMissing );
            if( Options.MultipleMainOptions() ) return this.CreateResult( OperationResultCode.MutlipleMainOptions );
            if( Options.IsMissingSubOption() ) return this.CreateResult( OperationResultCode.SubOptionMissing );
            if( Options.MultipleSubOptions() ) return this.CreateResult( OperationResultCode.MutlipleSubOptions );

            //verified most common mistakes, from here on, we will try to execute the operations.
            return await ProcessInternalAsync();
        }

        private async Task<OperationResult> ProcessInternalAsync()
        {
            if( Options.IsServiceOperation() )
            {
                if( Options.Create ) return await CreateServiceDefinitionAsync();
                if( Options.Read ) return await ReadServiceDefinitionAsync();
                if( Options.Delete ) return await DeleteServiceDefinitionAsync();
            }
            else if( Options.IsKeyValueOperation() )
            {
                ValidateKey();
                ValidateUri();
                await ValidateHostReachableAsync();

                if( Options.Create )
                {
                    ValidateValue();
                    return await CreateKeyValueAsync();
                }
                if( Options.Read ) return await ReadKeyAsync();
                if( Options.Delete ) return await DeleteKeyAsync();
            }
            return this.CreateResult( OperationResultCode.GenericError );
        }

        private async Task<OperationResult> DeleteServiceDefinitionAsync()
        {
            string serviceId = null;
            if( File.Exists( Options.ServiceArgument ) )
            {
                var serviceDef = ParseServiceDefinition();
                serviceId = serviceDef.Id;
                if( string.IsNullOrEmpty( serviceId ) )
                {
                    serviceId = serviceDef.Name;
                }
            }
            else
            {
                serviceId = Options.ServiceArgument;
            }

            ValidateUri();
            await ValidateHostReachableAsync();

            bool success = await Client.UnregisterServiceAsync( serviceId );
            return success ?
                this.CreateResult( OperationResultCode.Success ) :
                this.CreateResult( OperationResultCode.UnregisterServiceFailure );
        }

        private async Task<OperationResult> CreateServiceDefinitionAsync()
        {
            ServiceDefinition serviceDef = ParseServiceDefinition();
            ValidateUri();
            await ValidateHostReachableAsync();

            bool success = await Client.RegisterServiceAsync( serviceDef );
            return success ? 
                this.CreateResult( OperationResultCode.Success ) :
                this.CreateResult( OperationResultCode.RegisterServiceFailure );
        }

        private async Task<OperationResult> ReadServiceDefinitionAsync()
        {
            string serviceName = null;
            if( File.Exists( Options.ServiceArgument ) )
            {
                var serviceDef = ParseServiceDefinition();
                serviceName = serviceDef.Name;
            }
            else
            {
                serviceName = Options.ServiceArgument;
            }

            ValidateUri();
            await ValidateHostReachableAsync();

            var foundServices = await Client.ReadServicesByNameAsync( serviceName );

            return new OperationResult()
            {
                Success = true,
                ShowValue = true,
                Value = JsonUtils.GetPrettyPrintedJsonFromObject( foundServices ),
            };
        }

        private async Task<OperationResult> CreateKeyValueAsync()
        {
            string value = ParseValue();

            bool success = await Client.CreateKeyAsync( Options.Key, value );
            return success ? 
                this.CreateResult( OperationResultCode.Success ) :
                this.CreateResult( OperationResultCode.CreateKeyFailure );
        }

        private async Task<OperationResult> ReadKeyAsync()
        {
            var value = await Client.ReadKeySimpleAsync( Options.Key );

            return new OperationResult()
            {
                Success = true,
                ShowValue = true,
                Value = value,
            };
        }

        private async Task<OperationResult> DeleteKeyAsync()
        {
            bool success = await Client.DeleteKeyAsync( Options.Key );
            return success ?
                this.CreateResult( OperationResultCode.Success ) :
                this.CreateResult( OperationResultCode.DeleteKeyFailure );
        }
    }
}
