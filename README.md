# leandevops
Place to store various libraries to speed up creating VSTS/TFS CICD pipeline and various automation

**Microsoft.Sample.TfsReleaseHelper**
- Clone Release Environment &nbsp;
  * copy an existing environment created by hand and considered the "gold" template
  * also allows updating the deployment group
- Get Deployment Errors &nbsp;
  * Dump out the last x number of deploytment errors in JSON format including summary of the artifacts
- Get Tfs Release Environment Names &nbsp;
  * Dump out the environment names within a release definition. Useful when you have a ton of environments in a definition.
