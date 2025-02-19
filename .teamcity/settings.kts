import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.commitStatusPublisher
import jetbrains.buildServer.configs.kotlin.buildSteps.PowerShellStep
import jetbrains.buildServer.configs.kotlin.buildSteps.SSHUpload
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetTest
import jetbrains.buildServer.configs.kotlin.buildSteps.powerShell
import jetbrains.buildServer.configs.kotlin.buildSteps.sshExec
import jetbrains.buildServer.configs.kotlin.buildSteps.sshUpload

/*
The settings script is an entry point for defining a TeamCity
project hierarchy. The script should contain a single call to the
project() function with a Project instance or an init function as
an argument.

VcsRoots, BuildTypes, Templates, and subprojects can be
registered inside the project using the vcsRoot(), buildType(),
template(), and subProject() methods respectively.

To debug settings scripts in command-line, run the

    mvnDebug org.jetbrains.teamcity:teamcity-configs-maven-plugin:generate

command and attach your debugger to the port 8000.

To debug in IntelliJ Idea, open the 'Maven Projects' tool window (View
-> Tool Windows -> Maven Projects), find the generate task node
(Plugins -> teamcity-configs -> teamcity-configs:generate), the
'Debug' option is available in the context menu for the task.
*/

version = "2024.12"

project {

    buildType(Pets_TunnelGPT_Hosted_Build)
    buildType(Pets_TunnelGPT_Hosted_Deploy)

    params {
        text("branch_name", "hosted", readOnly = true, allowEmpty = true)
    }
}

object Pets_TunnelGPT_Hosted_Build : BuildType({
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
            scriptMode = script {
                content = """
                    #!/usr/bin/env pwsh
                    ${'$'}ErrorActionPreference = "Stop"
                    Set-StrictMode -Version Latest
                    ${'$'}appsettingsFile = "./TunnelGPT/appsettings.json"
                    try {
                        if (-not (Test-Path ${'$'}appsettingsFile)) {
                            Copy-Item ./TunnelGPT/appsettings.json.example ${'$'}appsettingsFile
                        }
                        ${'$'}json = Get-Content ${'$'}appsettingsFile -Raw | ConvertFrom-Json
                        ${'$'}json.OPENAI_API_KEY      = ${'$'}env:OPENAI_API_KEY
                        ${'$'}json.TELEGRAM_BOT_TOKEN  = ${'$'}env:TELEGRAM_BOT_TOKEN
                        ${'$'}json.TELEGRAM_BOT_SECRET = ${'$'}env:TELEGRAM_BOT_SECRET
                        ${'$'}json | ConvertTo-Json -Depth 10 | Set-Content ${'$'}appsettingsFile -Encoding UTF8
                    }
                    catch {
                        Write-Error "Failed to initialize appsettings.json. Reason:`n${'$'}_"
                        exit 1
                    }
                """.trimIndent()
            }
        }
        dotnetTest {
            name = "Test"
            id = "Test"
            coverage = dotcover {
                args = "-p:ContinuousIntegrationBuild=true"
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
    }
})

object Pets_TunnelGPT_Hosted_Deploy : BuildType({
    id("Deploy")
    name = "Deploy"

    enablePersonalBuilds = false
    type = BuildTypeSettings.Type.DEPLOYMENT
    buildNumberPattern = "${Pets_TunnelGPT_Hosted_Build.depParamRefs.buildNumber}"
    maxRunningBuilds = 1

    params {
        password("target_servername", "credentialsJSON:184ea855-6e00-41af-ba17-ae8170abc550")
        password("target_server_username", "credentialsJSON:baedf541-d196-49d5-bedc-416899708018")
    }

    vcs {
        root(DslContext.settingsRoot)

        checkoutMode = CheckoutMode.MANUAL
        cleanCheckout = true
        showDependenciesChanges = true
    }

    steps {
        sshExec {
            name = "Clear upload destination"
            id = "Clear_upload_destination"
            commands = """
                #!/bin/bash
                set -euo pipefail
                
                if ls /tmp/TunnelGPT_build*.zip &>/dev/null; then
                  rm /tmp/TunnelGPT_build*.zip
                fi
            """.trimIndent()
            targetUrl = "%target_servername%"
            authMethod = uploadedKey {
                username = "%target_server_username%"
                key = "oracle-cloud-instance-20250205-2240.key"
            }
        }
        sshUpload {
            name = "Upload"
            id = "Upload"
            transportProtocol = SSHUpload.TransportProtocol.SCP
            sourcePath = "TunnelGPT_build*.zip"
            targetUrl = "%target_servername%:/tmp"
            timeout = 600
            authMethod = uploadedKey {
                username = "%target_server_username%"
                key = "oracle-cloud-instance-20250205-2240.key"
            }
        }
        sshExec {
            name = "Install dependency packages"
            id = "ssh_exec_runner"
            commands = """
                #!/bin/bash
                set -euo pipefail
                
                export DEBIAN_FRONTEND=noninteractive
                
                # Install zip
                if which zip &>/dev/null; then
                  echo "zip is already installed";
                else
                  sudo apt-get update
                  sudo apt-get -y install zip
                fi
                
                # Install ASP.NET Core runtime
                dotnet_runtime_package="aspnetcore-runtime-9.0"
                if dpkg -s ${'$'}dotnet_runtime_package &>/dev/null; then
                    echo "${'$'}dotnet_runtime_package is already installed."
                else
                  sudo apt-get update
                  sudo apt-get install -y software-properties-common
                  sudo add-apt-repository -y ppa:dotnet/backports
                  sudo apt-get install -y ${'$'}dotnet_runtime_package
                fi
            """.trimIndent()
            targetUrl = "%target_servername%"
            authMethod = uploadedKey {
                username = "%target_server_username%"
                key = "oracle-cloud-instance-20250205-2240.key"
            }
        }
        sshExec {
            name = "Set up application"
            id = "Set_up_application"
            commands = """
                #!/bin/bash
                set -euo pipefail
                
                application_user="tunnelgpt"
                application_home="/opt/tunnelgpt"
                
                # Uninstall application
                if systemctl list-unit-files | grep '^tunnelgpt.service' &>/dev/null; then
                  sudo systemctl stop tunnelgpt
                fi
                if [ -d ${'$'}application_home ]; then
                  sudo rm -rf ${'$'}application_home
                fi
                
                # Initialize user
                if ! id "${'$'}application_user" &>/dev/null; then
                  sudo useradd -m -s /bin/bash "${'$'}application_user"
                fi
                
                # Initialize application home
                build_number="${Pets_TunnelGPT_Hosted_Build.depParamRefs.buildNumber}"
                sudo unzip /tmp/TunnelGPT_build${'$'}{build_number}.zip -d /opt/tunnelgpt
                sudo chown -R ${'$'}application_user:${'$'}application_user ${'$'}application_home
                sudo chmod 600 ${'$'}application_home/appsettings*.json
                rm /tmp/TunnelGPT_build${'$'}{build_number}.zip
                
                # Register service
                sudo tee /etc/systemd/system/tunnelgpt.service > /dev/null <<EOF
                [Unit]
                Description=TunnelGPT Telegram Bot
                After=network.target
                
                [Service]
                Type=simple
                User=${'$'}application_user
                WorkingDirectory=${'$'}application_home
                ExecStart=/usr/bin/dotnet ${'$'}application_home/TunnelGPT.dll
                Restart=always
                RestartSec=30
                
                [Install]
                WantedBy=multi-user.target
                EOF
                
                # Start service
                sudo systemctl daemon-reload
                sudo systemctl enable tunnelgpt
                sudo systemctl start tunnelgpt
            """.trimIndent()
            targetUrl = "%target_servername%"
            authMethod = uploadedKey {
                username = "%target_server_username%"
                key = "oracle-cloud-instance-20250205-2240.key"
            }
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
        dependency(Pets_TunnelGPT_Hosted_Build) {
            snapshot {
                runOnSameAgent = true
            }

            artifacts {
                artifactRules = "TunnelGPT_build*.zip"
            }
        }
    }
})
