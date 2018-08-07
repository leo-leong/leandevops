using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Sample.ReleaseUtil.TfsReleaseHelper
{
    public partial class TfsRelease
    {
        public bool CloneReleaseEnvironment(string SourceEnvName, string TargetEnvName)
        {
            bool success = false;

            var definitions = relclient.GetReleaseDefinitionsAsync(TfsEnvInfo.ProjectName, TfsEnvInfo.ReleaseDefinitionName, ReleaseDefinitionExpands.Environments, isExactNameMatch: true).Result;

            //This call is to get the project GUID
            var project = projclient.GetProject(TfsEnvInfo.ProjectName).Result;
            //This returns everything in a definition including properties such as Owner in an Environment
            var def = relclient.GetReleaseDefinitionAsync(project.Id, definitions.First().Id).Result;

            var envcount = def.Environments.Count;
            var source = def.Environments.Where(e => e.Name == SourceEnvName).First();

            //Environment must have a minimum RetentionPolicy, Rank, Owner, 
            //PreDeployApprovals, PostDeployApprovals and DeployPhases
            var target = new ReleaseDefinitionEnvironment()
            {
                Name = TargetEnvName,
                RetentionPolicy = source.RetentionPolicy,
                Rank = source.Rank++,
                Owner = source.Owner,
                PreDeployApprovals = source.PreDeployApprovals,
                PostDeployApprovals = source.PostDeployApprovals,
                DeployPhases = source.DeployPhases
            };
            def.Environments.Add(target);
            var newdef = relclient.UpdateReleaseDefinitionAsync(def, project.Id).Result;

            //Basic detection if environment was added
            if (newdef.Environments.Count > envcount)
                success = true;

            return success;
        }

        public bool CloneReleaseEnvironment(string SourceEnvName, string TargetEnvName, int DeployGroupId)
        {
            bool success = false;

            //This call is to get the project GUID
            var project = projclient.GetProject(TfsEnvInfo.ProjectName).Result;
            //This returns everything in a definition including properties such as Owner in an Environment
            var def = relclient.GetReleaseDefinitionAsync(project.Id, TfsEnvInfo.ReleaseDefinitionID).Result;

            var envcount = def.Environments.Count;
            var source = def.Environments.Where(e => e.Name == SourceEnvName).First();
            var newrank = def.Environments.Select(e => e.Rank).ToList().Max() + 1;
            
            //Environment must have a minimum RetentionPolicy, Rank, Owner, 
            //PreDeployApprovals, PostDeployApprovals and DeployPhases
            var target = new ReleaseDefinitionEnvironment()
            {
                Name = TargetEnvName + " - Clone from " + SourceEnvName,
                RetentionPolicy = source.RetentionPolicy,
                Rank = newrank,
                Owner = source.Owner,
                PreDeployApprovals = source.PreDeployApprovals,
                PostDeployApprovals = source.PostDeployApprovals
            };

            //Clone phases and tasks
            var newphases = source.DeployPhases.ToList();

            //Update deployment group name in phases
            foreach (var p in newphases)
            {
                if (p.PhaseType == DeployPhaseTypes.MachineGroupBasedDeployment)
                {
                    var m = ((MachineGroupBasedDeployPhase)p);
                    var inputs = m.GetDeploymentInput();
                    var newinputs = (MachineGroupDeploymentInput)inputs.Clone();
                    newinputs.QueueId = DeployGroupId;
                    m.DeploymentInput = newinputs;
                }
            }


            target.DeployPhases = newphases;
            def.Environments.Add(target);
            var newdef = relclient.UpdateReleaseDefinitionAsync(def, project.Id).Result;

            //Basic detection if environment was added
            if (newdef.Environments.Count > envcount)
                success = true;

            return success;
        }
    }
}
