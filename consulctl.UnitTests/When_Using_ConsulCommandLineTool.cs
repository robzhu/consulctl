using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Consul;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Consulctl.UnitTests
{
    public class MockConsulWebClient : IWebClient
    {
        public Uri Host { get; set; }
        public Func<bool> IsHostReachableCallback { get; set; }
        public Func<ServiceDefinition, bool> RegisterCallback { get; set; }
        public Func<string, bool> UnregisterCallback { get; set; }
        public MockConsulWebClient()
        {
            Host = new Uri( "localhost:8500" );
        }

        public void Dispose()
        {
        }

        public async Task<bool> IsHostReachableAsync()
        {
            return IsHostReachableCallback();
        }

        public async Task<bool> RegisterServiceAsync( ServiceDefinition service )
        {
            return RegisterCallback( service );
        }

        public async Task<bool> UnregisterServiceAsync( string serviceName )
        {
            return UnregisterCallback( serviceName );
        }

        public Task<bool> CreateKeyAsync( string key, string value )
        {
            throw new NotImplementedException();
        }

        public Task<ValueEntry[]> ReadKeyAsync( string key )
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadKeySimpleAsync( string key )
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteKeyAsync( string key )
        {
            throw new NotImplementedException();
        }

        public Task<ServiceDefinitionOutput[]> ReadServicesByNameAsync( string serviceName )
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class When_Using_ConsulCommandLineTool
    {
        const string NonExistentFile = "x:\\somefilethatdoesntexist.json";
        const string MalformedServiceDefinition = "bad_service_file.json";
        const string GoodServiceDefinition = "service.json";

        [TestMethod]
        public void Passing_null_args_shows_help()
        {
            var commandTool = new ConsulCommandLineTool();
            var result = commandTool.Process( null );

            Assert.IsTrue( result.Code == OperationResultCode.NoArguments );
            Assert.IsFalse( result.Success );
            Assert.IsTrue( result.ShowHelp );
        }

        [TestMethod]
        public void Passing_empty_args_shows_help()
        {
            var commandTool = new ConsulCommandLineTool();
            var result = commandTool.Process( new string[] { } );

            Assert.IsTrue( result.Code == OperationResultCode.NoArguments );
            Assert.IsFalse( result.Success );
            Assert.IsTrue( result.ShowHelp );
        }

        [TestMethod]
        public void Nonexistant_service_definition_path_shows_correct_error()
        {
            var commandTool = new ConsulCommandLineTool();
            var args = "-c -s DOESNOTEXIST.json";
            var result = commandTool.Process( args.Split() );

            Assert.IsTrue( result.Code == OperationResultCode.ServiceDefinitionFileNotFound );
            Assert.IsFalse( result.Success );
            Assert.IsFalse( result.ShowHelp );
        }

        [TestMethod]
        public void Badformat_service_definition_path_shows_correct_error()
        {
            var commandTool = new ConsulCommandLineTool();
            var args = "-c -s bad_service_file.json";
            var result = commandTool.Process( args.Split() );

            Assert.IsTrue( result.Code == OperationResultCode.ServiceDefinitionFileBadFormat );
            Assert.IsFalse( result.Success );
            Assert.IsFalse( result.ShowHelp );
        }

        [TestMethod]
        public void Unreachable_host_shows_correct_error()
        {
            var commandTool = new ConsulCommandLineTool()
            {
                Client = new MockConsulWebClient()
                {
                    IsHostReachableCallback = () => { return false; },
                }
            };

            var args = "--create --svc service.json -h 100.100.100.100";
            var result = commandTool.Process( args.Split() );

            Assert.IsTrue( result.Code == OperationResultCode.HostNotReachable );
            Assert.IsFalse( result.Success );
            Assert.IsFalse( result.ShowHelp );
        }

        [TestMethod]
        public void Invalid_host_shows_correct_error()
        {
            var commandTool = new ConsulCommandLineTool()
            {
                Client = new MockConsulWebClient()
                {
                    IsHostReachableCallback = () => { return false; },
                }
            };
            var args = "--create --svc service.json -h \"asdf.100.12312.100 a3kil4hj\"";
            var result = commandTool.Process( Split( args ) );

            Assert.IsTrue( result.Code == OperationResultCode.InvalidHostUri );
            Assert.IsFalse( result.Success );
            Assert.IsFalse( result.ShowHelp );
        }

        private string[] Split( string input )
        {
            var parts = Regex.Matches( input, @"[\""].+?[\""]|[^ ]+" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToArray();
            return parts;
        }

        [TestMethod]
        public void Missing_main_option_fails()
        {
            var commandTool = new ConsulCommandLineTool()
            {
                Client = new MockConsulWebClient()
                {
                    IsHostReachableCallback = () => { return true; },
                }
            };
            var result = commandTool.Process( new string[] { GoodServiceDefinition } );

            Assert.IsTrue( result.Code == OperationResultCode.MainOptionMissing );
            Assert.IsFalse( result.Success );
            Assert.IsFalse( result.ShowHelp );
        }

        [TestMethod]
        public void Specifying_multiple_main_args_yields_error()
        {
            var commandTool = new ConsulCommandLineTool()
            {
                Client = new MockConsulWebClient()
                {
                    IsHostReachableCallback = () => { return true; },
                }
            };
            var result = commandTool.Process( new string[] { "-s", GoodServiceDefinition, "--key", "meow" } );

            Assert.IsTrue( result.Code == OperationResultCode.MutlipleMainOptions );
            Assert.IsFalse( result.Success );
            Assert.IsFalse( result.ShowHelp );
        }

        [TestMethod]
        public void Missing_sub_option_fails()
        {
            var commandTool = new ConsulCommandLineTool()
            {
                Client = new MockConsulWebClient()
                {
                    IsHostReachableCallback = () => { return true; },
                    RegisterCallback = ( svc ) => { return true; },
                }
            };

            var args = "--svc service.json";
            var result = commandTool.Process( args.Split() );

            Assert.IsTrue( result.Code == OperationResultCode.SubOptionMissing );
            Assert.IsFalse( result.Success );
            Assert.IsFalse( result.ShowHelp );
        }

        [TestMethod]
        public void Multiple_sub_options_fails()
        {
            var commandTool = new ConsulCommandLineTool()
            {
                Client = new MockConsulWebClient()
                {
                    IsHostReachableCallback = () => { return true; },
                    RegisterCallback = ( svc ) => { return true; },
                }
            };

            var args = "--create --delete --svc service.json";
            var result = commandTool.Process( args.Split() );

            Assert.IsTrue( result.Code == OperationResultCode.MutlipleSubOptions );
            Assert.IsFalse( result.Success );
            Assert.IsFalse( result.ShowHelp );
        }
    }
}
