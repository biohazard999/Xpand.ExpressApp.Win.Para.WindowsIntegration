using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration
{
    /// <summary>
    ///     Enforces single instance for an application.
    /// </summary>
    public class SingleInstance : IDisposable
    {
        private readonly Boolean _OwnsMutex;
        private string _Identifier = Guid.Empty.ToString();
        private Mutex _Mutex;

        /// <summary>
        ///     Enforces single instance for an application.
        /// </summary>
        /// <param name="identifier">An identifier unique to this application.</param>
        public SingleInstance(string identifier)
        {
            _Identifier = identifier;

            _Mutex = new Mutex(true, identifier.ToString(CultureInfo.InvariantCulture), out _OwnsMutex);
        }

        /// <summary>
        ///     Indicates whether this is the first instance of this application.
        /// </summary>
        public Boolean IsFirstInstance
        {
            get { return _OwnsMutex || Arguments.Length == 0; }
        }

        /// <summary>
        ///     Passes the given arguments to the first running instance of the application.
        /// </summary>
        /// <param name="arguments">The arguments to pass.</param>
        /// <returns>Return true if the operation succeded, false otherwise.</returns>
        private Boolean PassArgumentsToFirstInstance(String[] arguments)
        {
            if (arguments == null)
                return false;

            if (!_OwnsMutex)
            {
                try
                {
                    using (var client = new NamedPipeClientStream(_Identifier))
                    using (var writer = new StreamWriter(client))
                    {
                        client.Connect(200);

                        foreach (String argument in arguments)
                        {
                            if (!string.IsNullOrEmpty(argument))
                            {
                                writer.WriteLine(argument);
                            }
                        }

                    }
                    return true;
                }
                catch (TimeoutException)
                {
                } //Couldn't connect to server
                catch (IOException)
                {
                } //Pipe was broken
            }

            return false;
        }

        /// <summary>
        ///     Listens for arguments being passed from successive instances of the applicaiton.
        /// </summary>
        public void ListenForArgumentsFromSuccessiveInstances()
        {
            if (_OwnsMutex)
                ThreadPool.QueueUserWorkItem(ListenForArguments);
        }

        /// <summary>
        ///     Listens for arguments on a named pipe.
        /// </summary>
        /// <param name="state">State object required by WaitCallback delegate.</param>
        private void ListenForArguments(Object state)
        {
            if (_OwnsMutex)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(_Identifier))
                    using (var reader = new StreamReader(server))
                    {
                        server.WaitForConnection();

                        var arguments = new List<String>();
                        while (server.IsConnected)
                        {
                            string result = reader.ReadLine();

                            if (!string.IsNullOrEmpty(result))
                                arguments.Add(result);
                        }


                        ThreadPool.QueueUserWorkItem(CallOnArgumentsReceived, arguments.ToArray());
                    }
                }
                catch (IOException)
                {
                } //Pipe was broken
                finally
                {
                    ListenForArguments(null);
                }
            }
        }

        /// <summary>
        ///     Calls the OnArgumentsReceived method casting the state Object to String[].
        /// </summary>
        /// <param name="state">The arguments to pass.</param>
        private void CallOnArgumentsReceived(Object state)
        {
            OnArgumentsReceived((String[])state);
        }

        /// <summary>
        ///     Event raised when arguments are received from successive instances.
        /// </summary>
        private static object _lock = new object();
        private EventHandler<ArgumentsReceivedEventArgs> _ArgumentsReceived = (sender, args) => { };
        public event EventHandler<ArgumentsReceivedEventArgs> ArgumentsReceived
        {
            add
            {
                lock (_lock)
                {
                    if (_OwnsMutex)
                    {
                        _ArgumentsReceived += value;
                    }
                }

            }
            remove
            {
                lock (_lock)
                {
                    _ArgumentsReceived -= value;
                }
            }
        }

        /// <summary>
        ///     Fires the ArgumentsReceived event.
        /// </summary>
        /// <param name="arguments">The arguments to pass with the ArgumentsReceivedEventArgs.</param>
        private void OnArgumentsReceived(String[] arguments)
        {
            _ArgumentsReceived(this, new ArgumentsReceivedEventArgs { Args = arguments });
        }

        #region IDisposable

        private Boolean disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (_Mutex != null && _OwnsMutex)
                {
                    _Mutex.ReleaseMutex();
                    _Mutex = null;
                }
                disposed = true;
            }
        }

        ~SingleInstance()
        {
            Dispose(false);
        }

        #endregion

        public void PassArgumentsToFirstInstance()
        {
            PassArgumentsToFirstInstance(Arguments);
        }

        private static string[] FilterAndAppendSlash(IEnumerable<string> args)
        {
            var listOfArgs = new List<string>();

            if (args == null)
                return listOfArgs.ToArray();

            foreach (var argument in args)
            {
                if (!string.IsNullOrEmpty(argument))
                {
                    if (Regex.IsMatch(argument, @"[\w-]{3,}://")) //Is Uri-Protocol
                        listOfArgs.Add(argument);
                    else
                    {
                        var arg = argument;
                        if (!arg.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
                            arg = "/" + arg;
                        listOfArgs.Add(arg);
                    }
                }
            }
            return listOfArgs.ToArray();
        }

        public static string[] Arguments
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed && AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null)
                {
                    var args = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;

                    if (args != null && args.Length >= 1)
                        args = args.Except(new[] { ApplicationDeployment.CurrentDeployment.UpdateLocation.ToString() }).ToArray();

                    return FilterAndAppendSlash(args);
                }

                var environmentArgs = Environment.GetCommandLineArgs();

                if (environmentArgs.Length >= 2)
                {
                    return FilterAndAppendSlash(environmentArgs.Skip(1));
                }

                return new string[0];
            }
        }
    }
}