import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.commitStatusPublisher
import jetbrains.buildServer.configs.kotlin.buildSteps.PowerShellStep
import jetbrains.buildServer.configs.kotlin.buildSteps.SSHUpload
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetTest
import jetbrains.buildServer.configs.kotlin.buildSteps.powerShell
import jetbrains.buildServer.configs.kotlin.buildSteps.sshExec
import jetbrains.buildServer.configs.kotlin.buildSteps.sshUpload

version = "2024.12"

project {

    buildType(Build)
    buildType(Deploy)

    params {
        text("branch_name", "hosted", readOnly = true, allowEmpty = false)
        password("target_servername", "credentialsJSON:184ea855-6e00-41af-ba17-ae8170abc550")
    }
}

object Build : BuildType({
    id("Build")
    name = "Build"

    artifactRules = "publish/** => TunnelGPT_build%build.number%.zip"

    params {
        password("env.OPENAI_API_KEY", "credentialsJSON:05a7b5b8-6ddb-4de4-b29e-41030e00a821")
        password("env.TELEGRAM_BOT_TOKEN", "credentialsJSON:c9ff78c9-8008-4061-9e8c-6e3325f39fcc")
        password("env.TELEGRAM_BOT_SECRET", "credentialsJSON:6c7c9f01-d820-4652-9580-90bae2f0d3df")
    }

    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        powerShell {
            name = "Initialize appsettings.json"
            id = "Initialize_appsettings_json"
            platform = PowerShellStep.Platform.x64
            edition = PowerShellStep.Edition.Core
            formatStderrAsError = true
            scriptMode = file {
                path = ".teamcity/init-appsettings.ps1"
            }
        }
        dotnetTest {
            name = "Test"
            id = "Test"
            args = "-p:ContinuousIntegrationBuild=true"
            coverage = dotcover {
                assemblyFilters = "-:*Tests"
            }
        }
        dotnetBuild {
            name = "Publish"
            id = "Publish"
            projects = "TunnelGPT"
            configuration = "Release"
            runtime = "linux-x64"
            outputDir = "publish"
            args = """
                --self-contained false -p:ContinuousIntegrationBuild=true
                -p:Version=1.0.0
                -p:FileVersion=1.0.0.%build.number%
                -p:AssemblyVersion=1.0.0.%build.number%
            """.trimIndent()
        }
        powerShell {
            name = "Create certificate"
            id = "Create_certificate"
            edition = PowerShellStep.Edition.Core
            formatStderrAsError = true
            scriptMode = file {
                path = ".teamcity/create-cert.ps1"
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
})

object Deploy : BuildType({
    val targetUploadDir = "/tmp/tunnelgpt"

    id("Deploy")
    name = "Deploy"

    enablePersonalBuilds = false
    type = Type.DEPLOYMENT
    buildNumberPattern = "${Build.depParamRefs.buildNumber}"
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
            commands = """
                #!/bin/bash
                set -euo pipefail
                
                if [ -d $targetUploadDir ]; then
                    rm -rf $targetUploadDir
                fi
            """.trimIndent()
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
                .teamcity/install-dependencies.sh
                .teamcity/install-tunnelgpt.sh
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
                sudo chmod +x $targetUploadDir/install-dependencies.sh
                sudo $targetUploadDir/install-dependencies.sh
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
                sudo chmod +x $targetUploadDir/install-tunnelgpt.sh
                sudo $targetUploadDir/install-tunnelgpt.sh "$targetUploadDir" "${Build.depParamRefs.buildNumber}"
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
                path = ".teamcity/verify-deployment.ps1"
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
        dependency(Build) {
            snapshot {
                runOnSameAgent = true
                onDependencyFailure = FailureAction.FAIL_TO_START
            }
            artifacts {
                artifactRules = "TunnelGPT_build*.zip"
            }
        }
    }
})
