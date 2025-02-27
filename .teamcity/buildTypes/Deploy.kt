package buildTypes

import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.commitStatusPublisher
import jetbrains.buildServer.configs.kotlin.buildSteps.PowerShellStep
import jetbrains.buildServer.configs.kotlin.buildSteps.SSHUpload
import jetbrains.buildServer.configs.kotlin.buildSteps.powerShell
import jetbrains.buildServer.configs.kotlin.buildSteps.sshExec
import jetbrains.buildServer.configs.kotlin.buildSteps.sshUpload

class Deploy(private val dependency: BuildBinaries) : BuildType({
    val depsInstallScriptCompileTimePath = "buildScripts/deploy/install-dependencies.sh"
    val appInstallScriptCompileTimePath = "buildScripts/deploy/install-tunnelgpt.sh"
    val verifyDeploymentScriptCompileTimePath = "buildScripts/deploy/verify-deployment.ps1"
    val depsInstallScriptName = depsInstallScriptCompileTimePath.substringAfterLast("/")
    val appInstallScriptName = appInstallScriptCompileTimePath.substringAfterLast("/")
    val targetUploadDir = "/tmp/tunnelgpt"

    id("Deploy")
    name = "Deploy"

    enablePersonalBuilds = false
    type = Type.DEPLOYMENT
    buildNumberPattern = "${dependency.depParamRefs.buildNumber}"
    maxRunningBuilds = 1

    params {
        password("target_server_username", "credentialsJSON:baedf541-d196-49d5-bedc-416899708018")
    }

    vcs {
        root(DslContext.settingsRoot, "+:.teamcity")

        cleanCheckout = true
        showDependenciesChanges = true
    }

    steps {
        sshExec {
            name = "Clear upload destination"
            id = "Clear_upload_destination"
            targetUrl = "%target_servername%"
            authMethod = uploadedKey {
                username = "%target_server_username%"
                key = "oracle-cloud-instance-20250205-2240.key"
            }
            commands = "if [ -d $targetUploadDir ]; then rm -rf $targetUploadDir; fi"
        }
        sshUpload {
            name = "Upload"
            id = "Upload"
            transportProtocol = SSHUpload.TransportProtocol.SCP
            targetUrl = "%target_servername%:$targetUploadDir"
            timeout = 600
            authMethod = uploadedKey {
                username = "%target_server_username%"
                key = "oracle-cloud-instance-20250205-2240.key"
            }
            sourcePath = """
                TunnelGPT_build*.zip
                .teamcity/$depsInstallScriptCompileTimePath
                .teamcity/$appInstallScriptCompileTimePath
            """.trimIndent()
        }
        sshExec {
            name = "Install dependency packages"
            id = "ssh_exec_runner"
            targetUrl = "%target_servername%"
            authMethod = uploadedKey {
                username = "%target_server_username%"
                key = "oracle-cloud-instance-20250205-2240.key"
            }
            commands = """
                sudo chmod +x $targetUploadDir/$depsInstallScriptName
                sudo $targetUploadDir/$depsInstallScriptName
            """.trimIndent()
        }
        sshExec {
            name = "Set up application"
            id = "Set_up_application"
            targetUrl = "%target_servername%"
            authMethod = uploadedKey {
                username = "%target_server_username%"
                key = "oracle-cloud-instance-20250205-2240.key"
            }
            commands = """
                sudo chmod +x $targetUploadDir/$appInstallScriptName
                sudo $targetUploadDir/$appInstallScriptName "${dependency.depParamRefs.buildNumber}"
            """.trimIndent()
        }
        sshExec {
            name = "Clear temp files"
            id = "Clear_temp_files"
            targetUrl = "%target_servername%"
            authMethod = uploadedKey {
                username = "%target_server_username%"
                key = "oracle-cloud-instance-20250205-2240.key"
            }
            commands = "rm -rf $targetUploadDir"
        }
        powerShell {
            name = "Verify deployment"
            id = "Verify_deployment"
            edition = PowerShellStep.Edition.Core
            formatStderrAsError = true
            scriptMode = file {
                path = ".teamcity/$verifyDeploymentScriptCompileTimePath"
            }
            scriptArgs = "-Servername %target_servername%"
        }
    }

    features {
        commitStatusPublisher {
            vcsRootExtId = "${DslContext.settingsRoot.id}"
            publisher = github {
                githubUrl = "https://api.github.com"
                authType = vcsRoot()
            }
        }
    }

    dependencies {
        dependency(dependency) {
            snapshot {
                onDependencyFailure = FailureAction.FAIL_TO_START
            }
            artifacts {
                artifactRules = "TunnelGPT_build*.zip"
            }
        }
    }
})