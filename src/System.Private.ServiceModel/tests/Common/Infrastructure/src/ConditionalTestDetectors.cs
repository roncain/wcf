// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace Infrastructure.Common
{
    // This class contains helper "detector" methods to detect specific
    // conditions about the environment in which the tests are run.
    // They are used for [ConditionalFact] and [ConditionalTheory]
    // tests.  They are called at most once, and the return value cached.
    internal static class ConditionalTestDetectors
    {
        public static bool IsRootCertificateInstalled()
        {
            try
            {
                ServiceUtilHelper.EnsureRootCertificateInstalled();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsClientCertificateInstalled()
        {
            try
            {
                ServiceUtilHelper.EnsureLocalClientCertificateInstalled();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Returns 'true' if the server is running IIS-hosted
        public static bool IsIISHosted()
        {
            return ServiceUtilHelper.IISHosted;
        }

        // Tests whether the client is domain-joined.  This test does not
        // consider whether the server is also domain-joined.
        public static bool IsClientDomainJoined()
        {
            // Currently non-Windows clients always return false until we have
            // a cross-platform way to detect this.
            if (!IsWindows())
            {
                return false;
            }

            // To determine whether the client is running on a domain joined machine,
            // we use this heuristic based on well known environment variables.
            string computerName = Environment.GetEnvironmentVariable("COMPUTERNAME");
            string logonServer = Environment.GetEnvironmentVariable("LOGONSERVER");
            string userDomain = Environment.GetEnvironmentVariable("USERDOMAIN");

            return !String.Equals(computerName, logonServer, StringComparison.OrdinalIgnoreCase) &&
                   !String.Equals(computerName, userDomain, StringComparison.OrdinalIgnoreCase);
        }

        // Tests whether the server is domain-joined.  This test does not consider
        // whether the client is domain-joined.
        public static bool IsServerDomainJoined()
        {
            // Requires solution to https://github.com/dotnet/wcf/issues/1095
            // Till then, heuristic is that running localhost == domain joined
            return IsServerLocalHost();
        }

        // Returns 'true' if the client is running on a Windows OS
        public static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        // Returns 'true' if the server is running as localhost.
        // This test is meant to be used only to detect that our service URI
        // indicates localhost.  It is not intended to be used to detect that
        // the services are running locally but using an explicit host name.
        public static bool IsServerLocalHost()
        {
            string host = TestProperties.GetProperty(TestProperties.ServiceUri_PropertyName);

            if (String.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            int index = host.IndexOf("localhost", 0, StringComparison.OrdinalIgnoreCase);
            return index == 0;
        }

        // Returns 'true' if ambient credentials are avaiable to use
        public static bool AreAmbientCredentialsAvailable()
        {
            // Requires solution to https://github.com/dotnet/wcf/issues/1095
            // Till then, heuristic is that running localhost == ambient credentials available
            return IsServerLocalHost();
        }

        // Returns 'true' if explicit credentials are available.
        // Currently, 'true' means the TestProperties for ExplicitUserName
        // and ExplicitPassword are non-blank.
        public static bool AreExplicitCredentialsAvailable()
        {
            return !String.IsNullOrWhiteSpace(TestProperties.GetProperty(TestProperties.ExplicitUserName_PropertyName)) &&
                   !String.IsNullOrWhiteSpace(TestProperties.GetProperty(TestProperties.ExplicitPassword_PropertyName));
        }

        // Returns 'true if SPN is available
        public static bool IsSPNAvailable()
        {
            // Requires solution to https://github.com/dotnet/wcf/issues/1095
            // Till then, heuristic is that running IIS-hosted on Windows means SPN is available
            return IsIISHosted() && IsWindows();
        }
    }
}
