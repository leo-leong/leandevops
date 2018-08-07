using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Sample.ReleaseUtil.TfsReleaseHelper
{
    public partial class TfsRelease
    {
        public List<string> GetTfsReleaseEnvironmentNames()
        {
            List<string> result = new List<string>();
            var definitions = relclient.GetReleaseDefinitionsAsync(TfsEnvInfo.ProjectName, TfsEnvInfo.ReleaseDefinitionName, ReleaseDefinitionExpands.Environments, isExactNameMatch: true).Result;
            var def = definitions.First();
            result = def.Environments.Select(e => e.Name).ToList();

            return result;
        }
    }
}
