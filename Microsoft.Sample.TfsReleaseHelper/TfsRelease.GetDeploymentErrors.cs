using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Sample.ReleaseUtil.TfsReleaseHelper
{
    public partial class TfsRelease
    {
        public string GetDeploymentErrors(int PastInstances)
        {
            string result = "";
            List<TfsReleaseError> errorlist = new List<TfsReleaseError>();
            Dictionary<string, string> inputs = new Dictionary<string, string>();

            var def = relclient.GetReleaseDefinitionsAsync(TfsEnvInfo.ProjectName, TfsEnvInfo.ReleaseDefinitionName, isExactNameMatch: true).Result;
            var id = def.First().Id;

            var runs = relclient.GetDeploymentsAsync(project: TfsEnvInfo.ProjectName, definitionId: id, deploymentStatus: DeploymentStatus.Failed, operationStatus: DeploymentOperationStatus.PhaseFailed, queryOrder: ReleaseQueryOrder.Descending, top: PastInstances).Result;

            foreach (var run in runs)
            {
                var rel = relclient.GetReleaseAsync(TfsEnvInfo.ProjectName, run.Release.Id).Result;
                var env = rel.Environments.First(e => e.Id == run.ReleaseEnvironmentReference.Id);
                var attempt = env.DeploySteps.First(s => s.Attempt == run.Attempt);
                var phase = attempt.ReleaseDeployPhases.First(p => p.Status == DeployPhaseStatus.Failed);

                //assumption here is each phase has only one job that failed
                var job = phase.DeploymentJobs.First(j => j.Job.Status == Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.TaskStatus.Failed);
                var failedtask = job.Tasks.Where(t => t.Status == Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.TaskStatus.Failed).First();

                var error = new TfsReleaseError()
                {
                    ReleaseName = rel.Name,
                    EnvironmentName = env.Name,
                    Attempt = attempt.Attempt,
                    PhaseType = phase.PhaseType.ToString(),
                    AgentName = failedtask.AgentName,
                    TaskName = failedtask.Name,
                    StartTime = failedtask.StartTime,
                    FinishTime = failedtask.FinishTime,
                    ErrorMessages = failedtask.Issues.Where(i => i.Message != "").Select(i => i.Message).ToArray()
                };

                var snapshot = env.DeployPhasesSnapshot.First(dps => dps.Rank == phase.Rank);
                if (failedtask.Task != null)    //failing at download artifacts
                {
                    inputs = snapshot.WorkflowTasks.First(w => w.TaskId == failedtask.Task.Id).Inputs;
                }

                error.TaskInputs = inputs;

                List<TfsArtifact> artifacts = new List<TfsArtifact>();
                foreach (var a in rel.Artifacts)
                {
                    var build = a.DefinitionReference.Where(d => d.Key == "version").Select(v => v.Value.Name).First();
                    var buildurl = a.DefinitionReference.Where(d => d.Key == "artifactSourceVersionUrl").Select(v => v.Value.Id).First();
                    artifacts.Add(new TfsArtifact()
                    {
                        Build = build,
                        BuildUrl = buildurl
                    });
                }
                error.Artifacts = artifacts.ToArray();

                errorlist.Add(error);
            }

            //To download all logs for a given release
            //docs: https://docs.microsoft.com/en-us/rest/api/vsts/release/releases/get%20logs?view=vsts-rest-4.1
            //GET https://{accountName}.vsrm.visualstudio.com/{project}/_apis/release/releases/{releaseId}/logs?api-version=4.1-preview.2
            //client.DownloadFile(rel.LogsContainerUrl, rel.Name + ".zip");
            //The return data seems to have bad encoding; looks like a bug
            //var logs = tfsclient.GetLogsAsync(tfsinfo.ProjectName, id).Result;

            result = JsonConvert.SerializeObject(errorlist, Formatting.Indented);

            return result;
        }
    }
}
