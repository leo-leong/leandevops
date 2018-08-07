using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

namespace Microsoft.Sample.ReleaseUtil.TfsReleaseHelper
{
    //Sample output
    //{
    //  "ReleaseName": "Release-1",
    //  "EnvironmentName": "Dev",
    //  "PhaseType": "AgentBasedDeployment",
    //  "Attempt": 1,
    //  "AgentName": "OnPrem-MachineName",
    //  "TaskName": "Deploy Azure App Service",
    //  "StartTime": "2018-07-13T22:01:11.7233333Z",
    //  "FinishTime": "2018-07-13T22:01:37Z",
    //  "ErrorMessages": [
    //  "Error: More than one package matched with specified pattern. Please restrain the search pattern."
    //  ],
    //  "TaskInputs": {
    //    "ConnectedServiceName": "$(Parameters.ConnectedServiceName)",
    //   ...
    public struct TfsReleaseError
    {
        public string ReleaseName;
        public string EnvironmentName;
        public string PhaseType;
        public int Attempt;
        public string AgentName;
        public string TaskName;
        public DateTime? StartTime;
        public DateTime? FinishTime;
        public string[] ErrorMessages;
        public Dictionary<string, string> TaskInputs;
        public TfsArtifact[] Artifacts;
    }

    public struct TfsArtifact
    {
        public string Build;
        public string BuildUrl;
    }

    public struct TfsInfo
    {
        public string ProjectCollectionUrl;
        public string ProjectName;
        public string ReleaseDefinitionName;
        public int ReleaseDefinitionID;
    }

    public partial class TfsRelease
    {
        private WebClient client;
        private ReleaseHttpClient relclient;
        private ProjectHttpClient projclient;

        public TfsInfo TfsEnvInfo { get; internal set; }

        public TfsRelease(TfsInfo TfsEnvInfo)
        {
            this.TfsEnvInfo = TfsEnvInfo;

            // Interactively ask the user for credentials, caching them so the user isn't constantly prompted
            VssCredentials credentials = new VssClientCredentials();
            credentials.Storage = new VssClientCredentialStorage();

            VssConnection connection = new VssConnection(new Uri(this.TfsEnvInfo.ProjectCollectionUrl), credentials);
            relclient = connection.GetClient<ReleaseHttpClient>();
            projclient = connection.GetClient<ProjectHttpClient>();

            client = new WebClient();
            client.Credentials = credentials.Windows.Credentials;
        }
    }
}
